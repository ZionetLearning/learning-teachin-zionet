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

# Function to clean up existing CRDs if needed
cleanup_conflicting_crds() {
    echo "Checking for conflicting CRDs..."
    
    # Check if HTTPScaledObject CRD exists and has conflicting ownership
    if kubectl get crd httpscaledobjects.http.keda.sh >/dev/null 2>&1; then
        CRD_NAMESPACE=$(kubectl get crd httpscaledobjects.http.keda.sh -o jsonpath='{.metadata.annotations.meta\.helm\.sh/release-namespace}' 2>/dev/null || echo "")
        CRD_RELEASE=$(kubectl get crd httpscaledobjects.http.keda.sh -o jsonpath='{.metadata.annotations.meta\.helm\.sh/release-name}' 2>/dev/null || echo "")
        
        if [ "$CRD_NAMESPACE" != "$KEDA_HTTP_NAMESPACE_TO_INSTALL" ] && [ -n "$CRD_NAMESPACE" ]; then
            echo "Found conflicting CRD ownership. CRD is owned by release '$CRD_RELEASE' in namespace '$CRD_NAMESPACE'"
            echo "Removing Helm ownership annotations to allow installation in new namespace..."
            
            # Remove Helm ownership annotations from CRDs
            kubectl annotate crd httpscaledobjects.http.keda.sh meta.helm.sh/release-name- meta.helm.sh/release-namespace- --overwrite || true
            
            # Also handle other KEDA HTTP CRDs if they exist
            kubectl annotate crd httpreplicas.http.keda.sh meta.helm.sh/release-name- meta.helm.sh/release-namespace- --overwrite 2>/dev/null || true
            
            echo "CRD ownership annotations removed"
        fi
    fi
}

# Function to install KEDA HTTP Add-on
install_keda_http() {
    local target_namespace=$1
    echo "Installing KEDA HTTP Add-on in namespace: $target_namespace..."
    
    # Create target namespace if it doesn't exist
    kubectl create namespace "$target_namespace" --dry-run=client -o yaml | kubectl apply -f -
    
    # Clean up any conflicting CRDs
    cleanup_conflicting_crds
    
    # Install KEDA HTTP Add-on with additional flags to handle CRD conflicts
    helm upgrade --install keda-http kedacore/keda-add-ons-http \
        --namespace "$target_namespace" \
        --set operator.keda.enabled=false \
        --set interceptor.kubernetes.watchNamespace="$target_namespace" \
        --set interceptor.kubernetes.interceptorService.namespaceOverride="$target_namespace" \
        --set scaler.kubernetes.kedaNamespace="$KEDA_CORE_NAMESPACE" \
        --skip-crds=false \
        --force \
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
kubectl get pods -n "$KEDA_HTTP_NAMESPACE_TO_INSTALL" -l app.kubernetes.io/name=keda-add-ons-http 2>/dev/null || echo "No KEDA HTTP pods found"

echo ""
echo "HTTPScaledObject CRD status:"
kubectl get crd httpscaledobjects.http.keda.sh -o jsonpath='{.metadata.name}: {.metadata.annotations.meta\.helm\.sh/release-name}@{.metadata.annotations.meta\.helm\.sh/release-namespace}' 2>/dev/null || echo "CRD not found"

echo ""
echo "KEDA installation/verification completed successfully!"
echo "KEDA Core: $KEDA_CORE_NAMESPACE namespace"
echo "KEDA HTTP: $KEDA_HTTP_NAMESPACE_TO_INSTALL namespace"