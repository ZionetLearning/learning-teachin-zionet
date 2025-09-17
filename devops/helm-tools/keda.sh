#!/bin/bash
set -e

KEDA_NAMESPACE="keda"

# Function to check if all KEDA pods are running and ready
check_keda_pods_ready() {
    echo "Waiting for all KEDA deployments to become Available..."
    if kubectl wait --for=condition=Available deployment --all -n "$KEDA_NAMESPACE" --timeout=300s; then
        echo "All KEDA deployments are available!"
        kubectl get pods -n "$KEDA_NAMESPACE"
        return 0
    else
        echo "Timeout waiting for KEDA deployments to be ready"
        kubectl get pods -n "$KEDA_NAMESPACE"
        return 1
    fi
}

# Check if KEDA is already installed and running
# if helm list -n "$KEDA_NAMESPACE" | grep -q "keda"; then
#     echo "KEDA Helm releases found, checking pod status..."
    
#     if check_keda_pods_ready; then
#         echo "KEDA is already installed and running, skipping installation..."
#         echo "KEDA verification complete!"
#         exit 0
#     else
#         echo "KEDA is installed but pods are not ready, proceeding with reinstallation..."
#     fi
# else
#     echo "KEDA not found, proceeding with fresh installation..."
# fi

echo "Installing KEDA Core and HTTP Add-on..."

# Add Helm repo
helm repo add kedacore https://kedacore.github.io/charts
helm repo update

# Create namespace
kubectl create namespace "$KEDA_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

# Install KEDA Core
echo "Installing KEDA Core..."
helm upgrade --install keda kedacore/keda \
    --namespace "$KEDA_NAMESPACE" \
    --wait --timeout 300s

# Install KEDA HTTP Add-on  
# echo "Installing KEDA HTTP Add-on..."
# helm upgrade --install keda-http kedacore/keda-add-ons-http \
#     --namespace "$KEDA_NAMESPACE" \
#     --set operator.keda.enabled=false \
#     --wait --timeout 300s