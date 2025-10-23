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

echo "Restarting Grafana to reload alerting files..."
kubectl rollout restart deployment "$DEPLOYMENT_NAME" -n "$NAMESPACE"

set +e
kubectl rollout status deployment "$DEPLOYMENT_NAME" -n "$NAMESPACE" --timeout="$ROLLOUT_TIMEOUT"
RC=$?
set -e

if [ $RC -ne 0 ]; then
  echo "Rollout timed out. Cleaning up old terminating pods and retrying..."
  NEW_HASH="$(kubectl -n "$NAMESPACE" get deploy "$DEPLOYMENT_NAME" -o jsonpath='{.spec.template.metadata.labels.pod-template-hash}')"
  kubectl -n "$NAMESPACE" get pods -l app.kubernetes.io/name=grafana \
    -o=jsonpath='{range .items[*]}{.metadata.name}{"|"}{.metadata.labels.pod-template-hash}{"|"}{.metadata.deletionTimestamp}{"\n"}{end}' \
    | awk -F"|" -v H="$NEW_HASH" '$2!=H || $3!="" {print $1}' \
    | xargs -r -n1 kubectl -n "$NAMESPACE" delete pod --force --grace-period=0

  kubectl rollout status deployment "$DEPLOYMENT_NAME" -n "$NAMESPACE" --timeout="${ROLLOUT_TIMEOUT/180s/420s}"
fi

echo "Alerting provisioned."