#!/usr/bin/env bash
set -euo pipefail

# ---------- required env ----------
: "${SUBSCRIPTION_ID:?SUBSCRIPTION_ID is required}"
: "${LA_WS_STUDENT:?LA_WS_STUDENT (Log Analytics workspace resourceId) is required}"
: "${LA_WS_ADMIN:?LA_WS_ADMIN (Log Analytics workspace resourceId) is required}"
: "${LA_WS_TEACHER:?LA_WS_TEACHER (Log Analytics workspace resourceId) is required}"

# Prefer NAMESPACE, then TARGET_NAMESPACE, then default
NAMESPACE="${NAMESPACE:-${TARGET_NAMESPACE:-devops-logs}}"
SRC="${SRC:-./provisioning/alerting/alerts-rules.yaml}"
TMP="/tmp/alerts-rules.rendered.yaml"

# ---------- make sure gettext (envsubst) exists ----------
if ! command -v envsubst >/dev/null 2>&1; then
  if command -v apt-get >/dev/null 2>&1; then
    sudo apt-get update -y && sudo apt-get install -y gettext-base
  else
    echo "ERROR: 'envsubst' (gettext) is required but not installed." >&2
    exit 1
  fi
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
kubectl -n "$NAMESPACE" rollout restart deployment/grafana || true

# ---------- rollout helper (tolerant) ----------
wait_for_rollout() {
  local ns="$1"
  local deploy="$2"
  local timeout="${3:-420s}"
  set +e
  kubectl -n "$ns" rollout status "deployment/${deploy}" --timeout="$timeout"
  local rc=$?
  set -e
  return $rc
}

# First wait; on timeout, collect diagnostics, clean terminating pods, and retry
if ! wait_for_rollout "$NAMESPACE" grafana 420s; then
  echo "Rollout timed out, collecting diagnostics..."
  kubectl -n "$NAMESPACE" get pods -l app.kubernetes.io/name=grafana -o wide || true
  kubectl -n "$NAMESPACE" get events --sort-by=.lastTimestamp | tail -n 50 || true

  echo "Force-deleting terminating old pods (if any)..."
  mapfile -t TERM_PODS < <(kubectl -n "$NAMESPACE" get pods -l app.kubernetes.io/name=grafana \
    -o jsonpath='{range .items[*]}{.metadata.name}{"|"}{.metadata.deletionTimestamp}{"\n"}{end}' \
    | awk -F"|" '$2!=""{print $1}')
  for p in "${TERM_PODS[@]:-}"; do
    kubectl -n "$NAMESPACE" delete pod "$p" --grace-period=0 --force || true
  done

  echo "Retrying rollout wait..."
  if ! wait_for_rollout "$NAMESPACE" grafana 420s; then
    echo "ERROR: Grafana rollout did not finish." >&2
    kubectl -n "$NAMESPACE" describe deploy/grafana || true
    kubectl -n "$NAMESPACE" describe pods -l app.kubernetes.io/name=grafana || true
    exit 1
  fi
fi

echo "Alerting provisioned."