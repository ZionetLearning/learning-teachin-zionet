#!/bin/bash
set -e

echo "Installing/Upgrading Dapr control plane..."

helm repo add dapr https://dapr.github.io/helm-charts || true
helm repo update

# Create namespace if it doesn't exist (ignore error if it already exists)
kubectl create ns dapr-system --dry-run=client -o yaml | kubectl apply -f -

helm upgrade --install dapr dapr/dapr \
  --version "1.15.9" \
  --namespace "dapr-system" \
  --set global.daprComponentInitTimeout="120s" \
  --wait

echo "Dapr control plane installation completed successfully!"