#!/bin/bash

set -e

K8S_DIR="./k8s"
NAMESPACE_FILE="$K8S_DIR/namespace-model.yaml"

# step 1: connect to azure
az aks get-credentials   --resource-group democuest-rg-dev   --name democuest-aks-dev   --overwrite-existing

# Step 2: Create namespace
echo "Creating namespace..."
kubectl apply -f "$NAMESPACE_FILE"

# Step 3: Apply secrets
echo "Applying secrets..."
kubectl apply -f "$K8S_DIR/secrets" --recursive

# Step 4: Apply Dapr components
echo "Applying Dapr components..."
kubectl apply -f "$K8S_DIR/dapr" --recursive

# Step 5: Apply services
echo "Applying services..."
kubectl apply -f "$K8S_DIR/services" --recursive

# Step 6: Apply deployments
export DOCKER_REGISTRY="benny902" # or read from env/.env file
echo "Applying deployments with registry substitution..."
find "$K8S_DIR/deployments" -type f -name '*.yaml' | while read file; do
  echo "Applying $file"
  envsubst < "$file" | kubectl apply -f -
done

echo "All resources applied successfully!"
