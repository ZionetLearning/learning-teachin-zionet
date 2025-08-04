#!/bin/bash
set -e

# ---- Configurable Section ----
NAMESPACE="devops-logs"
ADMIN_USER="admin"
ADMIN_PASS="admin123"
GRAFANA_CHART_VERSION="7.3.8"
MC_RG="MC_dev-zionet-learning-2025_aks-cluster-dev_westeurope"
IP_NAME="grafana-public-ip"
DNS_LABEL="grafana"
# -----------------------------

echo "0. Uninstalling existing Grafana Helm release (if present)..."
helm uninstall grafana -n "$NAMESPACE" || true
kubectl delete svc grafana -n "$NAMESPACE" || true

echo "1. Creating static public IP with Azure DNS label (safe if already exists)..."
az network public-ip create \
  --resource-group "$MC_RG" \
  --name "$IP_NAME" \
  --sku Standard

az network public-ip update \
  --resource-group "$MC_RG" \
  --name "$IP_NAME" \
  --dns-name "$DNS_LABEL"

echo "2. Add Grafana Helm repo (if missing) and update..."
helm repo add grafana https://grafana.github.io/helm-charts || true
helm repo update

echo "3. Create namespace if not exists..."
kubectl get ns "$NAMESPACE" >/dev/null 2>&1 || kubectl create ns "$NAMESPACE"

echo "4. Install/upgrade Grafana with static public IP, DNS annotation, and dashboard sidecar enabled..."
helm upgrade --install grafana grafana/grafana \
  --version "$GRAFANA_CHART_VERSION" \
  --namespace "$NAMESPACE" \
  --set adminUser="$ADMIN_USER" \
  --set adminPassword="$ADMIN_PASS" \
  --set persistence.enabled=true \
  --set persistence.size=5Gi \
  --set service.type=LoadBalancer \
  --set service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-resource-group"="$MC_RG" \
  --set service.annotations."service\.beta\.kubernetes\.io/azure-pip-name"="$IP_NAME" \
  --set sidecar.dashboards.enabled=true \
  --set sidecar.dashboards.searchNamespace="$NAMESPACE" \
  --wait

echo "5. Wait for Grafana external IP assignment..."
for i in {1..30}; do
  GRAFANA_IP=$(kubectl -n "$NAMESPACE" get svc grafana -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null)
  [[ -n "$GRAFANA_IP" ]] && break
  sleep 5
done
[[ -z "$GRAFANA_IP" ]] && { echo "Grafana IP not ready"; exit 1; }

echo
echo "✅ Grafana is available at:"
echo "   → http://$GRAFANA_IP"
echo "   → http://$DNS_LABEL.westeurope.cloudapp.azure.com"
echo
echo "Login:"
echo "   Username: $ADMIN_USER"
echo "   Password: $ADMIN_PASS"
