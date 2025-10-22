#!/usr/bin/env bash
set -euo pipefail

# ---------- required env ----------
: "${SUBSCRIPTION_ID:?SUBSCRIPTION_ID is required}"
: "${LA_WS_STUDENT:?LA_WS_STUDENT (Log Analytics workspace resourceId) is required}"
: "${LA_WS_ADMIN:?LA_WS_ADMIN (Log Analytics workspace resourceId) is required}"
: "${LA_WS_TEACHER:?LA_WS_TEACHER (Log Analytics workspace resourceId) is required}"

# Namespace defaults to where Grafana runs (do NOT derive from TARGET_NAMESPACE)
NAMESPACE="${NAMESPACE:-devops-logs}"
SRC="${SRC:-./provisioning/alerting/alerts-rules.yaml}"
TMP="/tmp/alerts-rules.rendered.yaml"

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
# Deployment name can be overridden via DEPLOYMENT_NAME; default to "grafana"
DEPLOYMENT_NAME="${DEPLOYMENT_NAME:-grafana}"

# If the default name isn't found, try to auto-detect by common label
if ! kubectl -n "$NAMESPACE" get deploy "$DEPLOYMENT_NAME" >/dev/null 2>&1; then
  CANDIDATE="$(kubectl -n "$NAMESPACE" get deploy -l app.kubernetes.io/name=grafana \
    -o jsonpath='{.items[0].metadata.name}' 2>/dev/null || true)"
  if [ -n "${CANDIDATE:-}" ]; then
    DEPLOYMENT_NAME="$CANDIDATE"
  else
    echo "ERROR: Grafana deployment not found in namespace '$NAMESPACE'." >&2
    echo "Hint: set NAMESPACE or DEPLOYMENT_NAME (e.g. DEPLOYMENT_NAME=kube-prometheus-stack-grafana)." >&2
    kubectl -n "$NAMESPACE" get deploy || true
    exit 1
  fi
fi

kubectl -n "$NAMESPACE" rollout restart "deployment/${DEPLOYMENT_NAME}"
kubectl -n "$NAMESPACE" rollout status  "deployment/${DEPLOYMENT_NAME}" --timeout=180s

echo "Alerting provisioned."