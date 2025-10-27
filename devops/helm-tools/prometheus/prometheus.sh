#!/bin/bash
set -e

# ==============================
# Configuration
# ==============================
GRAFANA_NAMESPACE="devops-logs"
PROM_NAMESPACE="monitoring"
PROM_CHART_VERSION="78.5.0"

# ==============================
# 1. Add Helm repos
# ==============================
echo "1. Adding Helm repositories..."
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts || true
helm repo update

# ==============================
# 2. Create namespaces
# ==============================
echo "2. Creating namespaces if needed..."
kubectl get ns $PROM_NAMESPACE >/dev/null 2>&1 || kubectl create ns $PROM_NAMESPACE

# ==============================
# 3. Install Prometheus stack
# ==============================
echo "3. Installing Prometheus stack..."
helm upgrade --install prom-stack prometheus-community/kube-prometheus-stack \
  --version "$PROM_CHART_VERSION" \
  --namespace "$PROM_NAMESPACE" \
  --values ./values-prometheus-dapr.yaml \
  --wait

# ==============================
# 4. Wait for Grafana
# ==============================
echo "4. Waiting for Grafana pod to be ready..."
kubectl wait --namespace "$GRAFANA_NAMESPACE" \
  --for=condition=Ready pod \
  --selector=app.kubernetes.io/name=grafana \
  --timeout=120s

# ==============================
# 5. Apply Grafana datasource
# ==============================
echo "5. Applying Grafana datasource..."
kubectl apply -f ./datasources.yaml -n "$GRAFANA_NAMESPACE"

# ==============================
# 6. Apply Grafana dashboards
# ==============================
echo "6. Applying Grafana dashboards..."
bash ../add-dashboards.sh ./dashboards

echo "Prometheus and Grafana successfully deployed and configured."