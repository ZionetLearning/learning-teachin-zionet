#!/bin/bash

set -e

K8S_DIR="./k8s for local with rabbitmq and cosmosdb-emulator/k8s"
NAMESPACE_FILE="$K8S_DIR/namespace-model.yaml"

echo "Checking prerequisites..."
command -v docker >/dev/null 2>&1 || { echo "Docker not found. Aborting."; exit 1; }
command -v kubectl >/dev/null 2>&1 || { echo "kubectl not found. Aborting."; exit 1; }

echo "Docker and kubectl are available."

echo "Enabling Kubernetes in Docker Desktop (make sure it's enabled in the UI)..."

# Step 1: Create namespace
echo "Creating namespace..."
kubectl apply -f "$NAMESPACE_FILE"

# Step 2: Install Dapr (if not installed)
if ! dapr --version >/dev/null 2>&1; then
  echo "Installing Dapr CLI..."
  wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash
fi

echo "Initializing Dapr on Kubernetes..."
dapr init -k

./wait-for-dapr.sh

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
