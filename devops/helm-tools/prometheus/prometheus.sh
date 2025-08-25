#!/bin/bash
set -e

# Define variables for namespaces, chart versions, and admin credentials
GRAFANA_NAMESPACE="devops-logs"
PROM_NAMESPACE="monitoring"
PROM_CHART_VERSION="57.0.2"

echo "1. Add Helm repos"
# Add the Prometheus Helm repo (ignore if it already exists), then update
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts || true
helm repo update

echo "2. Create namespaces if needed"
# Create the namespace for Prometheus if it doesn't exist
kubectl get ns $PROM_NAMESPACE >/dev/null 2>&1 || kubectl create ns $PROM_NAMESPACE

echo "3. Install/upgrade Prometheus Stack with Dapr metrics support"
# Deploy or upgrade the Prometheus stack using Helm with Dapr scraping configuration
helm upgrade --install prom-stack prometheus-community/kube-prometheus-stack \
  --version "$PROM_CHART_VERSION" \
  --namespace "$PROM_NAMESPACE" \
  --values ./values-prometheus-dapr.yaml \
  --wait  # Wait for all resources to be ready before continuing

echo "4. Wait for Grafana service to be ready"
# Wait for Grafana pod to be ready (optional safety step)
kubectl wait --namespace "$GRAFANA_NAMESPACE" \
  --for=condition=Ready pod \
  --selector=app.kubernetes.io/name=grafana \
  --timeout=120s

echo "5. Create Grafana datasource ConfigMap"
# Apply Grafana datasource configuration (datasources.yaml must exist in the working dir)
kubectl apply -f - <<EOF
apiVersion: v1
kind: ConfigMap
metadata:
  name: grafana-datasources
  namespace: $GRAFANA_NAMESPACE
  labels:
    grafana_datasource: "1"  # Grafana recognizes this label
data:
  datasources.yaml: |
$(sed 's/^/    /' datasources.yaml)  # Indent the contents of datasources.yaml correctly
EOF

echo "6. Create Grafana dashboard ConfigMaps"
# Loop over all dashboard JSON files and create a ConfigMap for each one
for file in dashboards/*.json; do
  DASH_NAME=$(basename "$file" .json)  # Sanitize name for ConfigMap
  kubectl apply -f - <<EOF
apiVersion: v1
kind: ConfigMap
metadata:
  name: dashboard-$DASH_NAME
  namespace: $GRAFANA_NAMESPACE
  labels:
    grafana_dashboard: "1"  # Grafana detects dashboards with this label
data:
  $DASH_NAME.json: |
$(sed 's/^/    /' "$file")  # Indent JSON content for YAML
EOF
done

echo "Prometheus stack deployed"
echo "Grafana dashboards and datasource configured"
