#!/bin/bash
set -e

KEDA_CORE_NAMESPACE="keda"
KEDA_HTTP_NAMESPACE_TO_INSTALL="$1"

if [ -z "$KEDA_HTTP_NAMESPACE_TO_INSTALL" ]; then
    echo "Error: Please provide the namespace where KEDA HTTP Add-on should be installed"
    echo "Usage: $0 <target-namespace>"
    exit 1
fi

# Generate a unique release name based on namespace
KEDA_HTTP_RELEASE="keda-http-${KEDA_HTTP_NAMESPACE_TO_INSTALL}"

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
    if helm list -n "$namespace" | grep -q "$KEDA_HTTP_RELEASE"; then
        echo "KEDA HTTP Add-on is already installed in namespace: $namespace"
        return 0
    else
        return 1
    fi
}

# Install KEDA HTTP Add-on with unique resource names
install_keda_http() {
    local target_namespace=$1
    echo "Installing KEDA HTTP Add-on in namespace: $target_namespace with release: $KEDA_HTTP_RELEASE"

    kubectl create namespace "$target_namespace" --dry-run=client -o yaml | kubectl apply -f -

    # Helm values to rename global resources per namespace
    helm upgrade --install "$KEDA_HTTP_RELEASE" kedacore/keda-add-ons-http \
        --namespace "$target_namespace" \
        --set operator.keda.enabled=false \
        --set interceptor.kubernetes.watchNamespace="$target_namespace" \
        --set interceptor.kubernetes.interceptorService.namespaceOverride="$target_namespace" \
        --set scaler.kubernetes.kedaNamespace="$KEDA_CORE_NAMESPACE" \
        --set global.clusterRoleName="${KEDA_HTTP_RELEASE}-interceptor-role" \
        --set global.clusterRoleBindingName="${KEDA_HTTP_RELEASE}-interceptor-binding" \
        --set global.proxyRoleName="${KEDA_HTTP_RELEASE}-proxy-role" \
        --set global.proxyRoleBindingName="${KEDA_HTTP_RELEASE}-proxy-binding" \
        --skip-crds=true \
        -f values-timeout.yaml \
        --wait --timeout 300s

    echo "KEDA HTTP Add-on installed successfully in namespace: $target_namespace"
}

echo "=== KEDA Installation Script ==="
echo "KEDA Core Namespace: $KEDA_CORE_NAMESPACE"
echo "KEDA HTTP Target Namespace: $KEDA_HTTP_NAMESPACE_TO_INSTALL"
echo "Helm Release Name: $KEDA_HTTP_RELEASE"
echo "================================="

# Add Helm repo
helm repo add kedacore https://kedacore.github.io/charts
helm repo update

# Check KEDA Core
if helm list -n "$KEDA_CORE_NAMESPACE" | grep -q "^keda\s"; then
    echo "KEDA Core Helm release found, checking pod status..."
    check_keda_core_ready || {
        echo "Reinstalling KEDA Core..."
        kubectl create namespace "$KEDA_CORE_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
        helm upgrade --install keda kedacore/keda --namespace "$KEDA_CORE_NAMESPACE" --wait --timeout 300s
    }
else
    echo "Installing KEDA Core..."
    kubectl create namespace "$KEDA_CORE_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
    helm upgrade --install keda kedacore/keda --namespace "$KEDA_CORE_NAMESPACE" --wait --timeout 300s
fi

# Install KEDA HTTP Add-on
if check_keda_http_installed "$KEDA_HTTP_NAMESPACE_TO_INSTALL"; then
    echo "KEDA HTTP Add-on already installed in $KEDA_HTTP_NAMESPACE_TO_INSTALL, skipping."
else
    install_keda_http "$KEDA_HTTP_NAMESPACE_TO_INSTALL"
fi

# Verification
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
echo "Helm Release Name: $KEDA_HTTP_RELEASE"