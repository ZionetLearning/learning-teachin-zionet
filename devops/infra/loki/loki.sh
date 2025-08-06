#!/bin/bash
set -e
NAMESPACE="devops-logs"
ADMIN_USER="admin"
ADMIN_PASS="admin123"

echo "1. Helm repo"
helm repo add grafana https://grafana.github.io/helm-charts || true
helm repo update

echo "2. Namespace"
kubectl get ns $NAMESPACE >/dev/null 2>&1 || kubectl create ns $NAMESPACE

echo "3. Dashboard ConfigMap"
kubectl apply -f ./pod-logs-dashboard.yaml

echo "4. Loki + Promtail (Grafana comes from Terraform)"
helm upgrade --install loki-stack grafana/loki-stack \
  -n $NAMESPACE \
  -f ./values-loki.yaml \
  --set grafana.enabled=false \
  --wait

# echo "5. Waiting for Grafana LoadBalancer IP …"
# for i in {1..30}; do
#   GRAFANA_IP=$(kubectl -n $NAMESPACE get svc grafana -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null)
#   [[ -n "$GRAFANA_IP" ]] && break
#   sleep 5
# done
# [[ -z "$GRAFANA_IP" ]] && { echo "Grafana IP not ready"; exit 1; }
# echo "Grafana IP = $GRAFANA_IP"

# echo "6. Waiting for Grafana API …"
# for i in {1..30}; do
#   code=$(curl -s -o /dev/null -w '%{http_code}' "http://$GRAFANA_IP/api/health" || true)
#   [[ "$code" == "200" ]] && break
#   sleep 5
# done
# [[ "$code" != "200" ]] && { echo "Grafana API not ready"; exit 1; }

# echo "7. Provision Loki datasource"
# curl -s -u "$ADMIN_USER:$ADMIN_PASS" -H "Content-Type: application/json" \
#      -X POST "http://$GRAFANA_IP/api/datasources" \
#      -d '{
#            "name":"Loki",
#            "type":"loki",
#            "url":"http://loki-stack:3100",
#            "access":"proxy",
#            "isDefault":true
#          }' >/dev/null

# echo "🔄 8. Force dashboard reload"
# curl -s -u "$ADMIN_USER:$ADMIN_PASS" \
#      -X POST "http://$GRAFANA_IP/api/admin/provisioning/dashboards/reload" >/dev/null

# echo -e "\nAll done — browse  http://$GRAFANA_IP  →  Dashboards ▸ Pod Logs"
