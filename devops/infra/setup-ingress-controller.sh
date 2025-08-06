#!/bin/bash
set -e

NAMESPACE="devops-ingress-nginx"
RELEASE_NAME="ingress-nginx"
STATIC_IP_NAME="ingress-controller-ip"
RESOURCE_GROUP="MC_dev-zionet-learning-2025-ingress_aks-cluster-dev_westeurope"
LOCATION="westeurope"

echo "0. Uninstalling existing ingress-nginx Helm release (if present)..."
helm uninstall "$RELEASE_NAME" -n "$NAMESPACE" || true

echo "1.1 Ensure Azure public IP exists..."
if ! az network public-ip show --resource-group "$RESOURCE_GROUP" --name "$STATIC_IP_NAME" &> /dev/null; then
  echo "Azure Public IP '$STATIC_IP_NAME' not found in '$RESOURCE_GROUP'. Creating..."
  az network public-ip create \
    --resource-group "$RESOURCE_GROUP" \
    --name "$STATIC_IP_NAME" \
    --sku Standard \
    --allocation-method static \
    --location "$LOCATION"
else
  echo "Azure Public IP '$STATIC_IP_NAME' already exists."
fi

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
  --set controller.service.type=LoadBalancer \
  --set controller.service.externalTrafficPolicy=Local \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"="/healthz" \
  --set-string controller.service.annotations."service\.beta\.kubernetes\.io/azure-pip-name"="$STATIC_IP_NAME" \
  --wait

echo "✅ Ingress Controller installed."

echo "Getting Ingress Controller External IP..."
EXTERNAL_IP=$(kubectl get svc ingress-nginx-controller -n $NAMESPACE -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "External IP: $EXTERNAL_IP"