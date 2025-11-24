#!/bin/bash
set -e

# ==============================
# Configuration
# ==============================
NAMESPACE="${NAMESPACE:-devops-logs}"
SRC="${SRC:-./provisioning/alerting/alerts-rules.yaml}"
DEPLOYMENT_NAME="${DEPLOYMENT_NAME:-grafana}"

# ==============================
# Create alerting ConfigMaps
# ==============================
echo "Applying Grafana alerting ConfigMaps..."
kubectl -n "$NAMESPACE" create configmap grafana-alerting \
  --from-file=alerts-rules.yaml="$SRC" --dry-run=client -o yaml | kubectl apply -f -
kubectl -n "$NAMESPACE" create configmap grafana-notifiers \
  --from-file=notifier-teams.yaml=./provisioning/notifiers/notifier-teams.yaml \
  --dry-run=client -o yaml | kubectl apply -f -
kubectl -n "$NAMESPACE" create configmap grafana-alerting-policy \
  --from-file=notification-policy.yaml=./provisioning/alerting/notification-policy.yaml \
  --dry-run=client -o yaml | kubectl apply -f -

echo "Alerting provisioned."