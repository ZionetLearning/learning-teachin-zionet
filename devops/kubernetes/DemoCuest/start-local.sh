#!/bin/bash

set -e

K8S_DIR="./k8s"
NAMESPACE_FILE="$K8S_DIR/namespace-model.yaml"
APP_CONFIG="$K8S_DIR/app-config.yaml"

echo "🔍 Checking prerequisites..."
command -v docker >/dev/null 2>&1 || { echo "❌ Docker not found. Aborting."; exit 1; }
command -v kubectl >/dev/null 2>&1 || { echo "❌ kubectl not found. Aborting."; exit 1; }

echo "✅ Docker and kubectl are available."

echo "🚀 Enabling Kubernetes in Docker Desktop (make sure it's enabled in the UI)..."

# Step 1: Create namespace
echo "📦 Creating namespace..."
kubectl apply -f "$NAMESPACE_FILE"

# Step 2: Install Dapr (if not installed)
if ! dapr --version >/dev/null 2>&1; then
  echo "📥 Installing Dapr CLI..."
  wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash
fi

echo "🔧 Initializing Dapr on Kubernetes..."
dapr init -k

./wait-for-dapr.sh

# Step 3: Apply secrets
echo "🔐 Applying secrets..."
kubectl apply -f "$K8S_DIR/secrets" --recursive

# Step 4: Apply Dapr components
echo "🧩 Applying Dapr components..."
kubectl apply -f "$K8S_DIR/dapr" --recursive

# Step 5: Apply services
echo "🌐 Applying services..."
kubectl apply -f "$K8S_DIR/services" --recursive

# Step 6: Apply deployments
echo "📦 Applying deployments..."
kubectl apply -f "$K8S_DIR/deployments" --recursive

# Step 7: Apply app config
#echo "⚙️ Applying app config..."
#kubectl apply -f "$APP_CONFIG"

echo "✅ All resources applied successfully!"
