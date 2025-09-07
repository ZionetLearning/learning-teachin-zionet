#!/bin/bash
set -e

KEDA_NAMESPACE="keda"

echo "Installing KEDA Core and HTTP Add-on..."

# Add Helm repo
helm repo add kedacore https://kedacore.github.io/charts
helm repo update

# Create namespace
kubectl create namespace "$KEDA_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

# Install KEDA Core
helm upgrade --install keda kedacore/keda \
    --namespace "$KEDA_NAMESPACE" \
    --wait

# Install KEDA HTTP Add-on  
helm upgrade --install keda-http kedacore/keda-add-ons-http \
    --namespace "$KEDA_NAMESPACE" \
    --set operator.keda.enabled=false \
    --wait

echo "Verifying installation..."
kubectl get pods -n "$KEDA_NAMESPACE"

# Apply KEDA ScaledObjects
echo "Applying KEDA ScaledObjects..."
kubectl apply -f ../kubernetes/charts/templates/keda --recursive


echo "KEDA installation complete!"