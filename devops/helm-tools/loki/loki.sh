#!/bin/bash
set -e

NAMESPACE="devops-logs"

echo "1. Helm repo"
helm repo add grafana https://grafana.github.io/helm-charts || true
helm repo update

echo "2. Namespace"
kubectl get ns $NAMESPACE >/dev/null 2>&1 || kubectl create ns $NAMESPACE

echo "3. Dashboard ConfigMaps"
kubectl apply -f ./pod-logs-dashboard.yaml

echo "4. Create Grafana dashboard ConfigMaps"
for file in dashboards/*.json; do
  DASH_NAME=$(basename "$file" .json)
  kubectl apply -f - <<EOF
apiVersion: v1
kind: ConfigMap
metadata:
  name: dashboard-$DASH_NAME
  namespace: $NAMESPACE
  labels:
    grafana_dashboard: "1"
data:
  $DASH_NAME.json: |
$(sed 's/^/    /' "$file")
EOF
done

echo "5. Loki + Promtail (Grafana comes from Terraform)"
helm upgrade --install loki-stack grafana/loki-stack \
  -n $NAMESPACE \
  -f ./values-loki.yaml \
  --set grafana.enabled=false \
  --wait

echo "6. Wait for Grafana service to be ready"
kubectl wait --namespace "$NAMESPACE" \
  --for=condition=Ready pod \
  --selector=app.kubernetes.io/name=grafana \
  --timeout=120s

echo "7. Create Loki datasource ConfigMap"
kubectl apply -f - <<EOF
apiVersion: v1
kind: ConfigMap
metadata:
  name: grafana-datasources-loki
  namespace: $NAMESPACE
  labels:
    grafana_datasource: "1"
data:
  loki-datasource.yaml: |
    apiVersion: 1
    datasources:
      - name: Loki
        type: loki
        url: http://loki-stack:3100
        access: proxy
        isDefault: false
        editable: true
EOF



echo -e "\n✅ All done — Loki datasource configured via ConfigMap"
echo "Browse your Grafana → Data Sources to see Loki datasource"