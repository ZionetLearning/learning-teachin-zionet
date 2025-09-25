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

# Function to ensure KEDA HTTP CRDs are installed globally
ensure_keda_http_crds() {
    echo "Checking KEDA HTTP CRDs..."
    
    if kubectl get crd httpscaledobjects.http.keda.sh >/dev/null 2>&1; then
        echo "HTTPScaledObject CRD already exists"
        
        # Check if CRD has Helm ownership annotations
        CRD_HELM_MANAGED=$(kubectl get crd httpscaledobjects.http.keda.sh -o jsonpath='{.metadata.annotations.meta\.helm\.sh/release-name}' 2>/dev/null || echo "")
        
        if [ -n "$CRD_HELM_MANAGED" ]; then
            echo "CRD is owned by Helm release. Removing ownership to allow multi-namespace installation..."
            
            # Remove Helm ownership annotations from HTTPScaledObject CRD
            kubectl annotate crd httpscaledobjects.http.keda.sh \
                meta.helm.sh/release-name- \
                meta.helm.sh/release-namespace- \
                --overwrite || true
            
            # Remove Helm ownership annotations from HTTPReplicas CRD if it exists
            kubectl annotate crd httpreplicas.http.keda.sh \
                meta.helm.sh/release-name- \
                meta.helm.sh/release-namespace- \
                --overwrite 2>/dev/null || true
            
            echo "Helm ownership removed from CRDs"
        fi
    else
        echo "Installing KEDA HTTP CRDs globally..."
        
        # Install CRDs directly without Helm ownership
        kubectl apply -f https://github.com/kedacore/http-add-on/releases/download/v0.8.0/keda-http-add-on-0.8.0-crds.yaml
        
        # Wait for CRDs to be established
        kubectl wait --for condition=established --timeout=60s crd/httpscaledobjects.http.keda.sh
        kubectl wait --for condition=established --timeout=60s crd/httpreplicas.http.keda.sh
        
        echo "KEDA HTTP CRDs installed successfully"
    fi
}

# Function to install KEDA HTTP Add-on
install_keda_http() {
    local target_namespace=$1
    echo "Installing KEDA HTTP Add-on in namespace: $target_namespace..."
    
    # Create target namespace if it doesn't exist
    kubectl create namespace "$target_namespace" --dry-run=client -o yaml | kubectl apply -f -
    
    # Ensure CRDs are available globally
    ensure_keda_http_crds
    
    # Check if values-timeout.yaml exists
    VALUES_FILE_PARAM=""
    if [ -f "values-timeout.yaml" ]; then
        echo "Found values-timeout.yaml file"
        VALUES_FILE_PARAM="-f values-timeout.yaml"
    else
        echo "values-timeout.yaml not found, proceeding without custom values"
    fi
    
    # Install KEDA HTTP Add-on without CRDs (skip them since they're already installed globally)
    echo "Running helm install command..."
    helm upgrade --install keda-http kedacore/keda-add-ons-http \
        --namespace "$target_namespace" \
        --set operator.keda.enabled=false \
        --set interceptor.kubernetes.watchNamespace="$target_namespace" \
        --set interceptor.kubernetes.interceptorService.namespaceOverride="$target_namespace" \
        --set scaler.kubernetes.kedaNamespace="$KEDA_CORE_NAMESPACE" \
        --skip-crds=true \
        $VALUES_FILE_PARAM \
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
kubectl get pods -n "$KEDA_HTTP_NAMESPACE_TO_INSTALL" -l app.kubernetes.io/name=keda-add-ons-http || echo "No KEDA HTTP pods found"

echo ""
echo "KEDA HTTP CRDs status:"
kubectl get crd | grep http.keda.sh || echo "No KEDA HTTP CRDs found"

echo ""
echo "KEDA installation/verification completed successfully!"
echo "KEDA Core: $KEDA_CORE_NAMESPACE namespace"
echo "KEDA HTTP: $KEDA_HTTP_NAMESPACE_TO_INSTALL namespace"
echo ""
echo "You can now create HTTPScaledObjects in namespace: $KEDA_HTTP_NAMESPACE_TO_INSTALL"
echo "Each namespace will have its own KEDA HTTP interceptor for proper isolation"