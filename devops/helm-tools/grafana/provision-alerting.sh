#!/usr/bin/env bash
set -euo pipefail

NAMESPACE="${NAMESPACE:-devops-logs}"
SRC="${SRC:-./provisioning/alerting/alerts-rules.yaml}"
DEPLOYMENT_NAME="${DEPLOYMENT_NAME:-grafana}"
ROLLOUT_TIMEOUT="${ROLLOUT_TIMEOUT:-180s}"

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