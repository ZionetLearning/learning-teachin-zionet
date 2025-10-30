#!/bin/bash
set -e

# ==============================
# Configuration
# ==============================
NAMESPACE="devops-logs"

# ==============================
# 1. Helm repo
# ==============================
echo "1. Helm repo"
helm repo add grafana https://grafana.github.io/helm-charts || true
helm repo update

# ==============================
# 2. Namespace
# ==============================
echo "2. Namespace"
kubectl get ns $NAMESPACE >/dev/null 2>&1 || kubectl create ns $NAMESPACE

# ==============================
# 3. Install Loki + Promtail
# ==============================
echo "3. Loki + Promtail (Grafana comes from Terraform)"
helm upgrade --install loki-stack grafana/loki-stack \
  -n $NAMESPACE \
  -f ./yaml/values-loki.yaml \
  --set grafana.enabled=false \
  --wait

# ==============================
# 4. Wait for Grafana service
# ==============================
echo "4. Wait for Grafana service to be ready"
kubectl wait --namespace "$NAMESPACE" \
  --for=condition=Ready pod \
  --selector=app.kubernetes.io/name=grafana \
  --timeout=120s

# ==============================
# 5. Apply Grafana datasource
# ==============================
echo "5. Applying Grafana datasource..."
kubectl apply -f ./yaml/datasources.yaml -n "$GRAFANA_NAMESPACE"

# ==============================
# 6. Apply Grafana dashboards
# ==============================
echo "6. Create Grafana dashboard ConfigMaps"
bash ../add-dashboards.sh ./dashboards

echo -e "\n All done — Loki datasource configured via ConfigMap"
echo "Browse your Grafana → Data Sources to see Loki datasource"