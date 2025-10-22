#!/usr/bin/env bash
set -euo pipefail

# ---------- required env ----------
: "${SUBSCRIPTION_ID:?SUBSCRIPTION_ID is required}"
: "${LA_WS_STUDENT:?LA_WS_STUDENT (Log Analytics workspace resourceId) is required}"
: "${LA_WS_ADMIN:?LA_WS_ADMIN (Log Analytics workspace resourceId) is required}"
: "${LA_WS_TEACHER:?LA_WS_TEACHER (Log Analytics workspace resourceId) is required}"

NAMESPACE="${NAMESPACE:-devops-logs}"
SRC="${SRC:-./provisioning/alerting/alerts-rules.yaml}"
TMP="/tmp/alerts-rules.rendered.yaml"
DEPLOYMENT_NAME="${DEPLOYMENT_NAME:-grafana}"
ROLLOUT_TIMEOUT="${ROLLOUT_TIMEOUT:-180s}"

# ---------- ensure gettext (envsubst) exists ----------
if ! command -v envsubst >/dev/null 2>&1; then
  sudo apt-get update -y && sudo apt-get install -y gettext-base
fi

echo "Rendering alerts from $SRC -> $TMP"
envsubst '${SUBSCRIPTION_ID} ${LA_WS_STUDENT} ${LA_WS_ADMIN} ${LA_WS_TEACHER}' < "$SRC" > "$TMP"

echo "Applying Grafana alerting ConfigMaps..."
kubectl -n "$NAMESPACE" create configmap grafana-alerting \
  --from-file=alerts-rules.yaml="$TMP" --dry-run=client -o yaml | kubectl apply -f -
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