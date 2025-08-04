#!/bin/bash
set -e

NAMESPACE="devops-logs"
ADMIN_USER="admin"
ADMIN_PASS="admin123"
GRAFANA_CHART_VERSION="7.3.8" # Change as desired

echo "1. Add Grafana Helm repo"
helm repo add grafana https://grafana.github.io/helm-charts || true
helm repo update

echo "2. Create namespace if not exists"
kubectl get ns $NAMESPACE >/dev/null 2>&1 || kubectl create ns $NAMESPACE

echo "3. Install/upgrade Grafana"
helm upgrade --install grafana grafana/grafana \
  --version "$GRAFANA_CHART_VERSION" \
  --namespace "$NAMESPACE" \
  --set adminUser="$ADMIN_USER" \
  --set adminPassword="$ADMIN_PASS" \
  --set persistence.enabled=true \
  --set persistence.size=5Gi \
  --set service.type=LoadBalancer \
  --wait

echo "4. Wait for external IP..."
for i in {1..30}; do
  GRAFANA_IP=$(kubectl -n "$NAMESPACE" get svc grafana -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null)
  [[ -n "$GRAFANA_IP" ]] && break
  sleep 5
done
[[ -z "$GRAFANA_IP" ]] && { echo "Grafana IP not ready"; exit 1; }
echo "Grafana is available at: http://$GRAFANA_IP"
echo "Login: $ADMIN_USER / $ADMIN_PASS"
