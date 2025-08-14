#!/usr/bin/env bash
set -euo pipefail
SECRETS=(azure-service-bus-secret postgres-connection signalr-connection redis-connection)
COMPONENTS=(statestore pubsub local-secret-store manager-to-engine engine-to-accessor engine-to-accessor-input manager-to-engine-input taskupdate taskupdate-input clientcallback ai-to-manager manager-to-ai)

read -rp "Source namespace [dev]: " SRC;  SRC="${SRC:-dev}"
read -rp "Target namespace [newdev]: " DST; DST="${DST:-newdev}"
[[ "$SRC" == "$DST" ]] && { echo "Source and target must differ."; exit 1; }
kubectl get ns "$DST" >/dev/null 2>&1 || kubectl create ns "$DST" >/dev/null

copy_kind() {
  local KIND="$1" NAME="$2"
  if ! kubectl get "$KIND" "$NAME" -n "$SRC" >/dev/null 2>&1; then
    echo "Skip $KIND/$NAME (not found in $SRC)"; return
  fi
  echo "Copy $KIND/$NAME  $SRC -> $DST"
  kubectl get "$KIND" "$NAME" -n "$SRC" -o yaml \
  | sed -e "s/namespace: ${SRC}/namespace: ${DST}/" \
        -e '/^[[:space:]]*uid:/d' \
        -e '/^[[:space:]]*resourceVersion:/d' \
        -e '/^[[:space:]]*creationTimestamp:/d' \
        -e '/^[[:space:]]*selfLink:/d' \
        -e '/^[[:space:]]*generation:/d' \
        -e '/^[[:space:]]*managedFields:/,/^[^[:space:]]/d' \
        -e '/^status:/,/^[^[:space:]]/d' \
  | kubectl apply -f - >/dev/null
}
for s in "${SECRETS[@]}";    do copy_kind secret    "$s"; done
for c in "${COMPONENTS[@]}"; do copy_kind component "$c"; done
echo "Done. Applied to namespace '$DST'."
