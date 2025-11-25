#!/bin/bash
set -e

# ==============================
# Configuration - Environment Detection
# ==============================
NAMESPACE="devops-ingress-nginx"
RELEASE_NAME="ingress-nginx"
STATIC_IP_NAME="ingress-controller-static-ip"
LOCATION="westeurope"
DNS_LABEL="teachin"

# Detect environment from context or parameter
ENVIRONMENT="${1:-dev}"  # Default to dev if no parameter passed

# Set resource group based on environment
if [ "$ENVIRONMENT" = "prod" ]; then
    MC_RG="MC_prod-zionet-learning-2025_aks-cluster-prod_westeurope"
    echo "🏭 Production environment detected"
else
    MC_RG="MC_dev-zionet-learning-2025_aks-cluster-dev_westeurope"
    echo "🔧 Development environment detected"
fi

echo "Using resource group: $MC_RG"

# ==============================
# 0. Uninstall existing ingress-nginx Helm release (if any)
# ==============================
echo "0. Uninstalling existing ingress-nginx Helm release (if present)..."
helm uninstall "$RELEASE_NAME" -n "$NAMESPACE" || true

# ==============================
# 1. Ensure Azure Public IP exists with DNS label
# ==============================
echo "1 Ensure Azure public IP exists..."
if ! az network public-ip show --resource-group "$MC_RG" --name "$STATIC_IP_NAME" &> /dev/null; then
  echo "Azure Public IP '$STATIC_IP_NAME' not found in '$MC_RG'. Creating..."
  az network public-ip create \
    --resource-group "$MC_RG" \
    --name "$STATIC_IP_NAME" \
    --sku Standard \
    --allocation-method static \
    --location "$LOCATION"
else
  echo "Azure Public IP '$STATIC_IP_NAME' already exists."
fi

# ==============================
# 2. Add DNS label to the Public IP
# ==============================
echo "2. Adding public DNS label to your static IP..."
az network public-ip update \
  --resource-group "$MC_RG" \
  --name "$STATIC_IP_NAME" \
  --dns-name "$DNS_LABEL"

# ==============================
# 3. Add ingress-nginx Helm repo
# ==============================
echo "3. Adding ingress-nginx Helm repo..."
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx || true
helm repo update

# ==============================
# 4. Create namespace
# ==============================
echo "4. Creating namespace $NAMESPACE (if not exists)..."
kubectl get ns "$NAMESPACE" >/dev/null 2>&1 || kubectl create ns "$NAMESPACE"

# ==============================
# 5. Install ingress-nginx Helm chart
# ==============================
echo "5. Installing ingress-nginx Helm chart..."
helm upgrade --install "$RELEASE_NAME" ingress-nginx/ingress-nginx \
  --namespace "$NAMESPACE" \
  --set controller.replicaCount=1 \
  --set controller.nodeSelector."kubernetes\.io/os"=linux \
  --set defaultBackend.nodeSelector."kubernetes\.io/os"=linux \
  --set controller.service.type=LoadBalancer \
  --set controller.service.externalTrafficPolicy=Local \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"="/healthz" \
  --set-string controller.service.annotations."service\.beta\.kubernetes\.io/azure-pip-name"="$STATIC_IP_NAME" \
  --set controller.config.enable-cors=true \
  --set controller.config.cors-allow-origin="*" \
  --set controller.config.cors-allow-credentials="true" \
  --wait

echo "✅ Ingress Controller installed."

echo "Getting Ingress Controller External IP..."
EXTERNAL_IP=$(kubectl get svc ingress-nginx-controller -n $NAMESPACE -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "External IP: $EXTERNAL_IP"

echo "✅ DNS label added. Your public address is:"
echo "    https://${DNS_LABEL}.${LOCATION}.cloudapp.azure.com"