#!/bin/bash
set -e

KEDA_NAMESPACE="keda"

# Function to check if all KEDA pods are running and ready
check_keda_pods_ready() {
    echo "Checking if KEDA pods are running and ready..."
    
    # Wait up to 5 minutes for pods to be ready
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        # Count total pods and ready pods
        local total_pods=$(kubectl get pods -n "$KEDA_NAMESPACE" --no-headers 2>/dev/null | wc -l)
        local ready_pods=$(kubectl get pods -n "$KEDA_NAMESPACE" --no-headers 2>/dev/null | grep -c "1/1.*Running\|2/2.*Running" || true)
        
        echo "Attempt $attempt/$max_attempts: $ready_pods/$total_pods pods ready"
        
        if [ $total_pods -gt 0 ] && [ $ready_pods -eq $total_pods ]; then
            echo "All KEDA pods are running and ready!"
            kubectl get pods -n "$KEDA_NAMESPACE"
            return 0
        fi
        
        echo "Waiting for pods to be ready..."
        sleep 10
        ((attempt++))
    done
    
    echo "Timeout waiting for KEDA pods to be ready"
    kubectl get pods -n "$KEDA_NAMESPACE"
    return 1
}

# Check if KEDA is already installed and running
if helm list -n "$KEDA_NAMESPACE" | grep -q "keda"; then
    echo "KEDA Helm releases found, checking pod status..."
    
    if check_keda_pods_ready; then
        echo "KEDA is already installed and running, skipping installation..."
        echo "KEDA verification complete!"
        exit 0
    else
        echo "KEDA is installed but pods are not ready, proceeding with reinstallation..."
    fi
else
    echo "KEDA not found, proceeding with fresh installation..."
fi

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
echo "Installing KEDA HTTP Add-on..."
helm upgrade --install keda-http kedacore/keda-add-ons-http \
    --namespace "$KEDA_NAMESPACE" \
    --set operator.keda.enabled=false \
    --wait --timeout 300s