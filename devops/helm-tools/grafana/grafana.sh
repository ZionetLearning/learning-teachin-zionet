#!/bin/bash
set -e

# ---- Configurable Section ----
NAMESPACE="devops-logs"
ADMIN_USER="admin"
ADMIN_PASS="admin123"
GRAFANA_CHART_VERSION="9.3.0"
MC_RG="MC_dev-zionet-learning-2025_aks-cluster-dev_westeurope"
# Public FQDN for ingress controller (must match ingress/grafana-ingress.yaml)
CONTROLLER_IP="teachin-zionet.westeurope.cloudapp.azure.com"
# -----------------------------

echo "0. Uninstalling existing Grafana Helm release (if present)..."
helm uninstall grafana -n "$NAMESPACE" || true
kubectl delete svc grafana -n "$NAMESPACE" || true

echo "2. Add Grafana Helm repo (if missing) and update..."
helm repo add grafana https://grafana.github.io/helm-charts || true
helm repo update

echo "3. Create namespace if not exists..."
kubectl get ns "$NAMESPACE" >/dev/null 2>&1 || kubectl create ns "$NAMESPACE"

echo "4. Creating ConfigMaps for Grafana provisioning..."
kubectl -n "$NAMESPACE" create configmap grafana-alerting --from-file=alert-rules.yaml=./alert-rules.yaml --dry-run=client -o yaml | kubectl apply -f -
kubectl -n "$NAMESPACE" create configmap grafana-notifiers --from-file=notifier-teams.yaml=./notifier-teams.yaml --dry-run=client -o yaml | kubectl apply -f -

echo "5. Install/upgrade Grafana with subpath configuration..."
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
  --set env.GF_SERVER_ROOT_URL="https://$CONTROLLER_IP/grafana/" \
  --set env.GF_SERVER_SERVE_FROM_SUB_PATH="true" \
  --set env.GF_SERVER_DOMAIN="$CONTROLLER_IP" \
  --set env.GF_INSTALL_PLUGINS="grafana-azure-monitor-datasource" \
  --set resources.requests.memory="128Mi" \
  --set resources.limits.memory="256Mi" \
  --set "extraConfigmapMounts[0].name=grafana-alerting" \
  --set "extraConfigmapMounts[0].configMap=grafana-alerting" \
  --set "extraConfigmapMounts[0].mountPath=/etc/grafana/provisioning/alerting" \
  --set "extraConfigmapMounts[0].readOnly=true" \
  --set "extraConfigmapMounts[1].name=grafana-notifiers" \
  --set "extraConfigmapMounts[1].configMap=grafana-notifiers" \
  --set "extraConfigmapMounts[1].mountPath=/etc/grafana/provisioning/notifiers" \
  --set "extraConfigmapMounts[1].readOnly=true" \
  --set env.TEAMS_WEBHOOK_URL="$TEAMS_WEBHOOK_URL" \
  --wait

echo
echo "✅ Grafana should be available at:"
echo "   → https://$CONTROLLER_IP/grafana/"
echo
echo "Login:"
echo "   Username: $ADMIN_USER"
echo "   Password: $ADMIN_PASS"
