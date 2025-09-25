#!/bin/bash
set -e

KEDA_CORE_NAMESPACE="keda"
KEDA_HTTP_NAMESPACE_TO_INSTALL="$1"

if [ -z "$KEDA_HTTP_NAMESPACE_TO_INSTALL" ]; then
    echo "Error: Please provide the namespace where KEDA HTTP Add-on should be installed"
    echo "Usage: $0 <target-namespace>"
    exit 1
fi

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

# Function to update ownership metadata for cluster-wide resources if they exist
update_cluster_resources() {
    local target_namespace=$1

    # CRD
    if kubectl get crd httpscaledobjects.http.keda.sh &>/dev/null; then
        echo "Updating ownership metadata for httpscaledobjects.http.keda.sh"
        kubectl annotate crd httpscaledobjects.http.keda.sh meta.helm.sh/release-name=keda-http --overwrite
        kubectl annotate crd httpscaledobjects.http.keda.sh meta.helm.sh/release-namespace="$target_namespace" --overwrite
        kubectl label crd httpscaledobjects.http.keda.sh app.kubernetes.io/managed-by=Helm --overwrite
    fi

    # ClusterRole
    if kubectl get clusterrole keda-add-ons-http-interceptor &>/dev/null; then
        echo "Updating ownership metadata for ClusterRole keda-add-ons-http-interceptor"
        kubectl annotate clusterrole keda-add-ons-http-interceptor meta.helm.sh/release-name=keda-http --overwrite
        kubectl annotate clusterrole keda-add-ons-http-interceptor meta.helm.sh/release-namespace="$target_namespace" --overwrite
        kubectl label clusterrole keda-add-ons-http-interceptor app.kubernetes.io/managed-by=Helm --overwrite
    fi

    # ClusterRoleBinding
    if kubectl get clusterrolebinding keda-add-ons-http-interceptor &>/dev/null; then
        echo "Updating ownership metadata for ClusterRoleBinding keda-add-ons-http-interceptor"
        kubectl annotate clusterrolebinding keda-add-ons-http-interceptor meta.helm.sh/release-name=keda-http --overwrite
        kubectl annotate clusterrolebinding keda-add-ons-http-interceptor meta.helm.sh/release-namespace="$target_namespace" --overwrite
        kubectl label clusterrolebinding keda-add-ons-http-interceptor app.kubernetes.io/managed-by=Helm --overwrite
    fi
}

# Function to install KEDA HTTP Add-on
install_keda_http() {
    local target_namespace=$1
    echo "Installing KEDA HTTP Add-on in namespace: $target_namespace..."
    
    # Create target namespace if it doesn't exist
    kubectl create namespace "$target_namespace" --dry-run=client -o yaml | kubectl apply -f -
    
    # Update ownership metadata for existing cluster-wide resources
    update_cluster_resources "$target_namespace"
    
    # Install KEDA HTTP Add-on
    helm upgrade --install keda-http kedacore/keda-add-ons-http \
        --namespace "$target_namespace" \
        --set operator.keda.enabled=false \
        --set interceptor.kubernetes.watchNamespace="$target_namespace" \
        --set interceptor.kubernetes.interceptorService.namespaceOverride="$target_namespace" \
        --set scaler.kubernetes.kedaNamespace="$KEDA_CORE_NAMESPACE" \
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
helm repo add kedacore https://kedacore.github.io/charts
helm repo update

# Check if KEDA Core is already installed and running
if helm list -n "$KEDA_CORE_NAMESPACE" | grep -q "^keda\s"; then
    echo "KEDA Core Helm release found, checking pod status..."
    
    if check_keda_core_ready; then
        echo "KEDA Core is already installed and running!"
    else
        echo "KEDA Core is installed but pods are not ready, reinstalling..."
        
        # Create namespace for KEDA Core
        kubectl create namespace "$KEDA_CORE_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
        
        # Reinstall KEDA Core
        echo "Reinstalling KEDA Core..."
        helm upgrade --install keda kedacore/keda \
            --namespace "$KEDA_CORE_NAMESPACE" \
            --wait --timeout 300s
        
        echo "KEDA Core reinstallation completed!"
    fi
else
    echo "KEDA Core not found, installing fresh..."
    
    # Create namespace for KEDA Core
    kubectl create namespace "$KEDA_CORE_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
    
    # Install KEDA Core
    echo "Installing KEDA Core..."
    helm upgrade --install keda kedacore/keda \
        --namespace "$KEDA_CORE_NAMESPACE" \
        --wait --timeout 300s
    
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