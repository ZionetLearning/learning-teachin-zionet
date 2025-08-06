#!/bin/bash
 
set -e
 
K8S_DIR="./kubernetes"
NAMESPACE_FILE="$K8S_DIR/namespaces/namespace-dev.yaml"
export DOCKER_REGISTRY="snir1551" # or read from env/.env file
 
# Step 1: Deploy infrastructure with Terraform
# echo "Deploying infrastructure with Terraform..."
# cd infra
# terraform apply -auto-approve
# cd ..
 
# Step 2: Connect to Azure AKS
az aks get-credentials --resource-group dev-zionet-learning-2025 --name aks-cluster-dev --overwrite-existing
 
# Set to cloud kubectl context
kubectl config use-context aks-cluster-dev
 
# Step 2: Create namespace
echo "Creating namespace..."
kubectl apply -f "$NAMESPACE_FILE"
 
# Step 3: Apply configurations and secrets
echo "Applying secrets..."
kubectl apply -f "$K8S_DIR/config" --recursive
 
# Step 4: Apply Dapr components
echo "Applying Dapr components..."
kubectl apply -f "$K8S_DIR/dapr/components" --recursive
 
# Step 5: Apply services
echo "Applying services..."
kubectl apply -f "$K8S_DIR/services" --recursive
 
# Step 6: Apply deployments
echo "Applying deployments with registry substitution..."
find "$K8S_DIR/deployments" -type f -name '*.yaml' | while read file; do
  echo "Applying $file"
  envsubst < "$file" | kubectl apply -f -
done

# Step 7: Apply KEDA ScaledObjects
echo "Applying KEDA ScaledObjects..."
find "$K8S_DIR/keda" -type f -name '*.yaml' | while read file; do
  echo "Applying $file"
  envsubst < "$file" | kubectl apply -f -
done
 
echo "All resources applied successfully!"
 
# Step 8: Wait for manager service to get an external IP
echo "Waiting for external IP for manager service..."
 
for i in {1..30}; do
  EXTERNAL_IP=$(kubectl -n dev get svc manager -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
 
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
  echo "You can now access the app at: http://$EXTERNAL_IP"
fi