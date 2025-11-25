#!/bin/bash
set -e

# ==============================
# Configuration - Environment Detection
# ==============================
NAMESPACE="devops-logs"
ADMIN_USER="admin"
ADMIN_PASS="admin123"
GRAFANA_CHART_VERSION="10.1.2"

# Detect environment from context or parameter
ENVIRONMENT="${1:-dev}"  # Default to dev if no parameter passed

# Set domain based on environment
if [ "$ENVIRONMENT" = "prod" ]; then
    CONTROLLER_IP="teachin-prod.westeurope.cloudapp.azure.com"
    echo "🏭 Production environment detected"
else
    CONTROLLER_IP="teachin.westeurope.cloudapp.azure.com"
    echo "🔧 Development environment detected"
fi

echo "Using domain: $CONTROLLER_IP"

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
echo "🕐 This may take up to 15 minutes for production environment..."

helm upgrade --install grafana grafana/grafana \
  --version "$GRAFANA_CHART_VERSION" \
  --namespace "$NAMESPACE" \
  -f ./yaml/grafana-values.yaml \
  --set adminUser="$ADMIN_USER" \
  --set adminPassword="$ADMIN_PASS" \
  --set sidecar.dashboards.searchNamespace="$NAMESPACE" \
  --set env.TEAMS_WEBHOOK_URL="$TEAMS_WEBHOOK_URL" \
  --set env.GF_SERVER_ROOT_URL="https://$CONTROLLER_IP/grafana/" \
  --set env.GF_SERVER_SERVE_FROM_SUB_PATH="true" \
  --set env.GF_SERVER_DOMAIN="$CONTROLLER_IP" \
  --timeout=15m \
  --wait

echo "✅ Grafana installation completed successfully!"

# ==============================
# 4. Checking Grafana pod status
# ==============================
echo "4. Checking Grafana pod status..."
kubectl get pods -n "$NAMESPACE" -l app.kubernetes.io/name=grafana

# Check if pods are ready
echo "📊 Waiting for Grafana pods to be ready..."
kubectl wait --for=condition=ready pod -l app.kubernetes.io/name=grafana -n "$NAMESPACE" --timeout=300s || {
  echo "⚠️ Grafana pods not ready within timeout. Checking events..."
  kubectl get events -n "$NAMESPACE" --sort-by='.lastTimestamp' | tail -20
  kubectl describe pod -n "$NAMESPACE" -l app.kubernetes.io/name=grafana
  echo "❌ Grafana deployment may have issues. Check the logs above."
}

echo "📋 Grafana service information:"
kubectl get svc -n "$NAMESPACE" -l app.kubernetes.io/name=grafana

echo
echo "✅ Grafana deployment completed!"
echo "🔗 Access URL: https://$CONTROLLER_IP/grafana/"
echo "👤 Login:"
echo "   Username: $ADMIN_USER"
echo "   Password: $ADMIN_PASS"