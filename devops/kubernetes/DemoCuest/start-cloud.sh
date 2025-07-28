#!/bin/bash

set -e


K8S_DIR="./k8s"
NAMESPACE_FILE="$K8S_DIR/namespace-model.yaml"

# step 1: connect to azure
az aks get-credentials   --resource-group democuest-aks-rg-dev   --name democuest-aks-dev   --overwrite-existing

# set to cloud kubcetl context
kubectl config use-context democuest-aks-dev

# Step 2: Create namespace
echo "Creating namespace..."
kubectl apply -f "$NAMESPACE_FILE"

# Step 3: Apply secrets
# echo "Applying secrets..."
# kubectl apply -f "$K8S_DIR/secrets" --recursive

# Step 4: Apply Dapr components
echo "Applying Dapr components..."
kubectl apply -f "$K8S_DIR/dapr/components" --recursive

# Step 5: Apply services
echo "Applying services..."
kubectl apply -f "$K8S_DIR/services" --recursive

# Step 5.5: Apply shared resources (PVCs, etc.)
echo "Applying shared resources..."
kubectl apply -f "$K8S_DIR/shared" --recursive

# Step 6: Apply deployments
export DOCKER_REGISTRY="snir1551" # or read from env/.env file
echo "Applying deployments with registry substitution..."
find "$K8S_DIR/deployments" -type f -name '*.yaml' | while read file; do
  echo "Applying $file"
  envsubst < "$file" | kubectl apply -f -
done

echo "All resources applied successfully!"

# Step 7: Wait for manager service to get an external IP
echo "Waiting for external IP for manager service..."

for i in {1..30}; do
  EXTERNAL_IP=$(kubectl -n devops-model get svc manager -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
  
  if [[ -n "$EXTERNAL_IP" ]]; then
    echo "External IP is ready: $EXTERNAL_IP"
    break
  fi

  echo "Attempt $i: External IP not yet assigned. Waiting 10s..."
  sleep 10
done

if [[ -z "$EXTERNAL_IP" ]]; then
  echo "Failed to get external IP after waiting. Check 'kubectl get svc manager'"
else
  echo "You can now access the app at: http://$EXTERNAL_IP:5073"
fi