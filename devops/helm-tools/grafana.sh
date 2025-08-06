#!/bin/bash
set -e

# ---- Configurable Section ----
NAMESPACE="devops-logs"
ADMIN_USER="admin"
ADMIN_PASS="admin123"
GRAFANA_CHART_VERSION="9.3.0"
MC_RG="MC_dev-zionet-learning-2025_aks-cluster-dev_westeurope"
# The following will fetch the static public IP from Azure
IP_NAME="grafana-public-ip"
DNS_LABEL="grafana"
CONTROLLER_IP=$(az network public-ip show --resource-group "$MC_RG" --name "$IP_NAME" --query ipAddress -o tsv)
# -----------------------------

echo "0. Uninstalling existing Grafana Helm release (if present)..."
helm uninstall grafana -n "$NAMESPACE" || true
kubectl delete svc grafana -n "$NAMESPACE" || true

echo "2. Add Grafana Helm repo (if missing) and update..."
helm repo add grafana https://grafana.github.io/helm-charts || true
helm repo update

echo "3. Create namespace if not exists..."
kubectl get ns "$NAMESPACE" >/dev/null 2>&1 || kubectl create ns "$NAMESPACE"

echo "4. Install/upgrade Grafana with subpath configuration..."
helm upgrade --install grafana grafana/grafana \
  --version "$GRAFANA_CHART_VERSION" \
  --namespace "$NAMESPACE" \
  --set adminUser="$ADMIN_USER" \
  --set adminPassword="$ADMIN_PASS" \
  --set persistence.enabled=true \
  --set persistence.size=5Gi \
  --set service.type=ClusterIP \
  --set sidecar.dashboards.enabled=true \
  --set sidecar.dashboards.searchNamespace="$NAMESPACE" \
  --set sidecar.datasources.enabled=true \
  --set env.GF_SERVER_ROOT_URL="http://$CONTROLLER_IP/grafana/" \
  --set env.GF_SERVER_SERVE_FROM_SUB_PATH="true" \
  --set env.GF_SERVER_DOMAIN="$CONTROLLER_IP" \
  --wait

echo
echo "✅ Grafana should be available at:"
echo "   → http://$CONTROLLER_IP/grafana/"
echo
echo "Login:"
echo "   Username: $ADMIN_USER"
echo "   Password: $ADMIN_PASS"
