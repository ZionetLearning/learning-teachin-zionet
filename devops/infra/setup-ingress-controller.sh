#!/bin/bash
set -e

NAMESPACE="devops-ingress-nginx"
RELEASE_NAME="ingress-nginx"

echo "0. Uninstalling existing ingress-nginx Helm release (if present)..."
helm uninstall "$RELEASE_NAME" -n "$NAMESPACE" || true

echo "1. Adding ingress-nginx Helm repo..."
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx || true
helm repo update

echo "2. Creating namespace $NAMESPACE (if not exists)..."
kubectl get ns "$NAMESPACE" >/dev/null 2>&1 || kubectl create ns "$NAMESPACE"

echo "3. Installing ingress-nginx Helm chart..."
helm upgrade --install "$RELEASE_NAME" ingress-nginx/ingress-nginx \
  --namespace "$NAMESPACE" \
  --set controller.replicaCount=1 \
  --set controller.nodeSelector."kubernetes\.io/os"=linux \
  --set defaultBackend.nodeSelector."kubernetes\.io/os"=linux \
  --wait

echo "✅ Ingress Controller installed."

echo "Getting Ingress Controller External IP..."
EXTERNAL_IP=$(kubectl get svc ingress-nginx-controller -n $NAMESPACE -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "External IP: $EXTERNAL_IP"