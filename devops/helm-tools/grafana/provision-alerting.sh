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

# ---------- make sure gettext (envsubst) exists ----------
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
kubectl rollout restart deployment grafana -n "$NAMESPACE"
kubectl rollout status  deployment grafana -n "$NAMESPACE" --timeout=180s

echo "Alerting provisioned."