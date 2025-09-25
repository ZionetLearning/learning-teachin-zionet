#!/bin/bash
set -e

KEDA_CORE_NAMESPACE="keda"
KEDA_HTTP_NAMESPACE_TO_INSTALL="$1"

if [ -z "$KEDA_HTTP_NAMESPACE_TO_INSTALL" ]; then
    echo "Error: Please provide the namespace where KEDA HTTP Add-on should be installed"
    echo "Usage: $0 <target-namespace>"
    exit 1
fi

# Suffix used to make global resources unique per namespace
RESOURCE_SUFFIX="$KEDA_HTTP_NAMESPACE_TO_INSTALL"

# Function to check if all KEDA Core pods are running and ready
check_keda_core_ready() {
    echo "Checking KEDA Core deployments in namespace: $KEDA_CORE_NAMESPACE"
    if kubectl wait --for=condition=Available deployment --all -n "$KEDA_CORE_NAMESPACE" --timeout=300s --selector=app.kubernetes.io/name=keda-operator 2>/dev/null; then
        echo "KEDA Core deployments are available!"
        kubectl get pods -n "$KEDA_CORE_NAMESPACE" -l app.kubernetes.io/name=keda-operator
        return 0
    else
        echo "KEDA Core deployments are not ready or don't exist"
        return 1
    fi
}

# Function to check if KEDA HTTP Add-on is already installed in target namespace
check_keda_http_installed() {
    local namespace=$1
    if helm list -n "$namespace" | grep -q "keda-http"; then
        echo "KEDA HTTP Add-on is already installed in namespace: $namespace"
        return 0
    else
        return 1
    fi
}

# Function to update ownership metadata for existing global resources
update_global_resource_ownership() {
    local kind=$1
    local name=$2
    local namespace=$3

    if kubectl get "$kind" "$name" &>/dev/null; then
        echo "Updating ownership metadata for $kind $name"
        kubectl annotate "$kind" "$name" meta.helm.sh/release-name=keda-http --overwrite
        kubectl annotate "$kind" "$name" meta.helm.sh/release-namespace="$namespace" --overwrite
        kubectl label "$kind" "$name" app.kubernetes.io/managed-by=Helm --overwrite || true
    fi
}

# Function to install KEDA HTTP Add-on
install_keda_http() {
    local target_namespace=$1
    echo "Installing KEDA HTTP Add-on in namespace: $target_namespace..."

    # Create target namespace if it doesn't exist
    kubectl create namespace "$target_namespace" --dry-run=client -o yaml | kubectl apply -f -

    # Update ownership metadata for known global resources
    for res in \
        httpscaledobjects.http.keda.sh \
        keda-add-ons-http-interceptor \
        keda-add-ons-http-role \
        keda-add-ons-http-rolebinding; do
        update_global_resource_ownership "$res" "$res" "$target_namespace"
    done

    # Install KEDA HTTP Add-on with unique suffix for ClusterRole names
    helm upgrade --install keda-http kedacore/keda-add-ons-http \
        --namespace "$target_namespace" \
        --set operator.keda.enabled=false \
        --set interceptor.kubernetes.watchNamespace="$target_namespace" \
        --set interceptor.kubernetes.interceptorService.namespaceOverride="$target_namespace" \
        --set scaler.kubernetes.kedaNamespace="$KEDA_CORE_NAMESPACE" \
        --set global.resourceSuffix="$RESOURCE_SUFFIX" \
        --skip-crds=true \
        -f values-timeout.yaml \
        --wait --timeout 300s

    echo "KEDA HTTP Add-on installed successfully in namespace: $target_namespace"
}

echo "=== KEDA Installation Script ==="
echo "KEDA Core Namespace: $KEDA_CORE_NAMESPACE"
echo "KEDA HTTP Target Namespace: $KEDA_HTTP_NAMESPACE_TO_INSTALL"
echo "================================="

# Add Helm repo (always do this to ensure latest charts)
echo "Adding/updating KEDA Helm repository..."
helm repo add kedacore https://kedacore.github.io/charts || true
helm repo update

# Check if KEDA Core is already installed and running
if helm list -n "$KEDA_CORE_NAMESPACE" | grep -q "^keda\s"; then
    echo "KEDA Core Helm release found, checking pod status..."
    check_keda_core_ready || {
        echo "KEDA Core is installed but pods are not ready, reinstalling..."
        kubectl create namespace "$KEDA_CORE_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
        helm upgrade --install keda kedacore/keda --namespace "$KEDA_CORE_NAMESPACE" --wait --timeout 300s
        echo "KEDA Core reinstallation completed!"
    }
else
    echo "KEDA Core not found, installing fresh..."
    kubectl create namespace "$KEDA_CORE_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
    helm upgrade --install keda kedacore/keda --namespace "$KEDA_CORE_NAMESPACE" --wait --timeout 300s
    echo "KEDA Core installation completed!"
fi

# Check if KEDA HTTP Add-on is already installed in target namespace
if check_keda_http_installed "$KEDA_HTTP_NAMESPACE_TO_INSTALL"; then
    echo "KEDA HTTP Add-on is already installed in namespace: $KEDA_HTTP_NAMESPACE_TO_INSTALL"
    echo "Skipping HTTP Add-on installation..."
else
    echo "KEDA HTTP Add-on not found in namespace: $KEDA_HTTP_NAMESPACE_TO_INSTALL"
    install_keda_http "$KEDA_HTTP_NAMESPACE_TO_INSTALL"
    echo "KEDA HTTP Add-on installation completed!"
fi

# Final verification
echo ""
echo "=== Final Verification ==="
echo "KEDA Core status:"
kubectl get pods -n "$KEDA_CORE_NAMESPACE" -l app.kubernetes.io/name=keda-operator

echo ""
echo "KEDA HTTP Add-on status in $KEDA_HTTP_NAMESPACE_TO_INSTALL:"
kubectl get pods -n "$KEDA_HTTP_NAMESPACE_TO_INSTALL" -l app.kubernetes.io/name=keda-add-ons-http

echo ""
echo "KEDA installation/verification completed successfully!"
echo "KEDA Core: $KEDA_CORE_NAMESPACE namespace"
echo "KEDA HTTP: $KEDA_HTTP_NAMESPACE_TO_INSTALL namespace"