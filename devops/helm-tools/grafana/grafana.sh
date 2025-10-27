#!/bin/bash
set -e

# ==============================
# Configuration
# ==============================
NAMESPACE="devops-logs"
ADMIN_USER="admin"
ADMIN_PASS="admin123"
GRAFANA_CHART_VERSION="10.1.2"
CONTROLLER_IP="teachin.westeurope.cloudapp.azure.com"

# ==============================
# Delete existing Grafana release
# ==============================
echo "0. Uninstalling existing Grafana Helm release (if present)..."
helm uninstall grafana -n "$NAMESPACE" || true
kubectl delete svc grafana -n "$NAMESPACE" || true

# ==============================
# 1. Helm repo
# ==============================
echo "1. Add Grafana Helm repo (if missing) and update..."
helm repo add grafana https://grafana.github.io/helm-charts || true
helm repo update

# ==============================
# 2. Create namespace
# ==============================
echo "2. Create namespace if not exists..."
kubectl get ns "$NAMESPACE" >/dev/null 2>&1 || kubectl create ns "$NAMESPACE"

# ==============================
# 3. Install/upgrade Grafana
# ==============================
echo "3. Install/upgrade Grafana with subpath configuration..."
helm upgrade --install grafana grafana/grafana \
  --version "$GRAFANA_CHART_VERSION" \
  --namespace "$NAMESPACE" \
  -f ./yaml/grafana-values.yaml \
  --set adminUser="$ADMIN_USER" \
  --set adminPassword="$ADMIN_PASS" \
  --set sidecar.dashboards.searchNamespace="$NAMESPACE" \
  --set env.TEAMS_WEBHOOK_URL="$TEAMS_WEBHOOK_URL" \
  --set env.GF_SERVER_SERVE_FROM_SUB_PATH="true" \
  --timeout=10m \
  --wait

# ==============================
# 4. Checking Grafana pod status
# ==============================
echo "4. Checking Grafana pod status..."
kubectl get pods -n "$NAMESPACE" -l app.kubernetes.io/name=grafana
kubectl describe pod -n "$NAMESPACE" -l app.kubernetes.io/name=grafana | grep -A 10 "Events:"

echo
echo "Login:"
echo "   Username: $ADMIN_USER"
echo "   Password: $ADMIN_PASS"