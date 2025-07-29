#!/bin/bash

set -e

LOKI_NAMESPACE="devops-logs"

# Step 1: Add Helm repo (if not already added)
helm repo add grafana https://grafana.github.io/helm-charts || true
helm repo update

# Step 2: Apply dashboard configmap
kubectl apply -f ./loki/pod-logs-dashboard.yaml

# Step 3: Install/upgrade Loki + Promtail ONLY
helm upgrade --install loki-stack grafana/loki-stack \
  --namespace "$LOKI_NAMESPACE" \
  -f ./loki/values-loki.yaml \
  --set grafana.enabled=false \
  --wait

echo ""
echo "✅ Loki/Promtail setup complete!"
echo "🌍 Grafana is already running at your LoadBalancer service."
echo "📊 Dashboard: Pod Logs (will appear automatically in the UI)"


echo ""
echo "Waiting for external IP for Grafana service..."

for i in {1..30}; do
  EXTERNAL_IP=$(kubectl -n devops-logs get svc grafana -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

  if [[ -n "$EXTERNAL_IP" ]]; then
    echo "External IP is ready: $EXTERNAL_IP"
    break
  fi

  echo "Attempt $i: External IP not yet assigned. Waiting 10s..."
  sleep 10
done

if [[ -z "$EXTERNAL_IP" ]]; then
  echo "Failed to get external IP after waiting. Check 'kubectl get svc grafana -n devops-logs'"
  echo "You can try port-forwarding: kubectl port-forward svc/grafana 3000:80 -n devops-logs"
else
  echo "🌍 You can now access Grafana at: http://$EXTERNAL_IP/"
  echo "👤 Login: admin / admin123"
fi
