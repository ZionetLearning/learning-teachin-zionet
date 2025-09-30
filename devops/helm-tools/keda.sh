#!/usr/bin/env bash
set -euo pipefail

# ------------------------------------------------------------------------------
# KEDA Core (once-per-cluster) + KEDA HTTP add-on (per-namespace) installer
#
# What this script does:
#   - Ensures the target namespace exists
#   - Ensures Helm repos exist
#   - Detects whether KEDA Core CRDs already exist (cluster-scoped)
#       * If missing: installs KEDA Core ONCE and creates CRDs (cluster owner)
#         - watchNamespace="" so Core watches all namespaces
#         - CloudEvents/Eventing disabled to avoid extra CRDs
#       * If present: skips Core installation
#   - Installs/Upgrades the KEDA HTTP add-on PER NAMESPACE
#   - Pre-adopts (annotates/labels) cluster-scoped HTTP add-on resources (CRD & RBAC)
#     to the current Helm release to avoid "invalid ownership metadata" errors
#   - Creates per-namespace ClusterRoleBindings for the HTTP add-on ServiceAccounts
#     (interceptor / external-scaler / operator), idempotently
#
# Usage:
#   ./keda.sh <namespace> [keda_chart_version] [http_addon_chart_version]
#
# Examples:
#   ./keda.sh featest
#   ./keda.sh testkeda 2.17.2 0.11.0
#
# Notes:
#   - CRDs are cluster-scoped and must exist only once per cluster.
#   - The first run creates them; subsequent runs skip them.
#   - This script keeps using "-f values-timeout.yaml" as requested.
# ------------------------------------------------------------------------------

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <namespace> [keda_chart_version] [http_addon_chart_version]"
  exit 1
fi

NS="$1"
KEDA_VER="${2:-}"   # optional: pin kedacore/keda chart version (e.g., 2.17.2)
HTTP_VER="${3:-}"   # optional: pin kedacore/keda-add-ons-http chart version (e.g., 0.11.0)

# Helm release names (unique per namespace)
REL_CORE="keda-${NS}"        # used only for the FIRST (core) install
REL_HTTP="keda-http-${NS}"   # per-namespace HTTP add-on release

# Required tools
command -v kubectl >/dev/null 2>&1 || { echo "kubectl not found"; exit 1; }
command -v helm    >/dev/null 2>&1 || { echo "helm not found"; exit 1; }

echo "[*] Namespace: ${NS}"
kubectl get ns "${NS}" >/dev/null 2>&1 || kubectl create ns "${NS}"

echo "[*] Helm repos"
helm repo add kedacore https://kedacore.github.io/charts >/dev/null 2>&1 || true
helm repo update >/dev/null 2>&1 || true

# ----------------------------- helpers ---------------------------------------

# Returns 0 if ALL given CRDs exist; 1 otherwise.
all_crds_exist() {
  local missing=0
  for crd in "$@"; do
    if ! kubectl get crd "$crd" >/dev/null 2>&1; then
      missing=1
      break
    fi
  done
  return $missing
}

# Adopt a CRD to the current Helm release (avoids Helm "ownership" conflicts)
adopt_crd_to_release() {
  local crd="$1" rel="$2" ns="$3"
  if ! kubectl get crd "$crd" >/dev/null 2>&1; then
    return 0
  fi
  local cur_rel cur_ns
  cur_rel="$(kubectl get crd "$crd" -o jsonpath='{.metadata.annotations.meta\.helm\.sh/release-name}' 2>/dev/null || true)"
  cur_ns="$( kubectl get crd "$crd" -o jsonpath='{.metadata.annotations.meta\.helm\.sh/release-namespace}' 2>/dev/null || true)"
  if [[ "$cur_rel" == "$rel" && "$cur_ns" == "$ns" ]]; then
    return 0
  fi
  echo "[*] Adopting CRD ${crd} -> ${rel}/${ns}"
  kubectl annotate crd "$crd" \
    "meta.helm.sh/release-name=${rel}" \
    "meta.helm.sh/release-namespace=${ns}" --overwrite || true
  kubectl label crd "$crd" \
    "app.kubernetes.io/managed-by=Helm" --overwrite || true
}

# Adopt cluster-scoped RBAC objects of the HTTP add-on to this release.
# WARNING: This re-assigns Helm ownership to the current release to avoid
# "cannot be imported" errors when installing per-namespace. If you later
# uninstall a previous release, Helm will not try to remove these globals.
adopt_http_cluster_scoped() {
  local rel="$1" ns="$2"
  # ClusterRoles
  for obj in \
    "clusterrole/keda-add-ons-http-interceptor" \
    "clusterrole/keda-add-ons-http-external-scaler" \
    "clusterrole/keda-add-ons-http-role"
  do
    if kubectl get "${obj}" >/dev/null 2>&1; then
      echo "[*] Adopting ${obj} -> ${rel}/${ns}"
      kubectl annotate "${obj}" \
        "meta.helm.sh/release-name=${rel}" \
        "meta.helm.sh/release-namespace=${ns}" --overwrite || true
      kubectl label "${obj}" \
        "app.kubernetes.io/managed-by=Helm" --overwrite || true
    fi
  done

  # ClusterRoleBindings created by the HTTP chart can also collide across releases.
  # Adopt them too so the new install won't fail on ownership checks.
  for obj in \
    "clusterrolebinding/keda-add-ons-http-interceptor" \
    "clusterrolebinding/keda-add-ons-http-external-scaler" \
    "clusterrolebinding/keda-add-ons-http-role"
  do
    if kubectl get "${obj}" >/dev/null 2>&1; then
      echo "[*] Adopting ${obj} -> ${rel}/${ns}"
      kubectl annotate "${obj}" \
        "meta.helm.sh/release-name=${rel}" \
        "meta.helm.sh/release-namespace=${ns}" --overwrite || true
      kubectl label "${obj}" \
        "app.kubernetes.io/managed-by=Helm" --overwrite || true
    fi
  done
}

# Ensure per-namespace ClusterRoleBindings for the HTTP add-on SAs (idempotent).
# These bindings grant the SAs the cluster-scoped permissions they need (List/Watch, etc.)
ensure_http_namespace_crbs() {
  local ns="$1"

  # Names are made unique per-namespace to avoid collisions.
  cat <<EOF | kubectl apply -f -
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: keda-http-interceptor-${ns}
subjects:
- kind: ServiceAccount
  name: keda-add-ons-http-interceptor
  namespace: ${ns}
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: keda-add-ons-http-interceptor
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: keda-http-external-scaler-${ns}
subjects:
- kind: ServiceAccount
  name: keda-add-ons-http-external-scaler
  namespace: ${ns}
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: keda-add-ons-http-external-scaler
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: keda-http-operator-${ns}
subjects:
- kind: ServiceAccount
  name: keda-add-ons-http
  namespace: ${ns}
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: keda-add-ons-http-role
EOF
}

# ----------------------------- CRDs/Core -------------------------------------

CORE_CRDS=(
  scaledobjects.keda.sh
  scaledjobs.keda.sh
  triggerauthentications.keda.sh
  clustertriggerauthentications.keda.sh
)

HTTP_CRD="httpscaledobjects.http.keda.sh"

FIRST_CORE_INSTALL=0
if all_crds_exist "${CORE_CRDS[@]}"; then
  echo "[*] KEDA Core CRDs already exist -> Core is already installed"
else
  echo "[*] KEDA Core CRDs are missing -> FIRST Core install (cluster owner)"
  FIRST_CORE_INSTALL=1
fi

if [[ "${FIRST_CORE_INSTALL}" -eq 1 ]]; then
  echo "[*] Installing KEDA Core (cluster-wide owner of CRDs)"
  helm upgrade --install "${REL_CORE}" kedacore/keda \
    ${KEDA_VER:+--version "$KEDA_VER"} \
    -n "${NS}" \
    --set watchNamespace="" \
    --set fullnameOverride="${REL_CORE}" \
    --set crds.create=true \
    --set cloudEvents.enabled=false \
    --set cloudevents.enabled=false \
    --set eventing.enabled=false \
    --timeout 900s --atomic --debug
else
  echo "[*] Skipping KEDA Core install in '${NS}' (CRDs already exist)"
fi

# ----------------------------- HTTP add-on -----------------------------------

echo "[*] Preparing to install/upgrade KEDA HTTP add-on in namespace '${NS}'"

# If the HTTP CRD already exists, adopt it to this release and skip CRD creation.
if kubectl get crd "${HTTP_CRD}" >/dev/null 2>&1; then
  adopt_crd_to_release "${HTTP_CRD}" "${REL_HTTP}" "${NS}"
  HTTP_CRDS_FLAG="--set crds.create=false"
  HTTP_SKIP_FLAG="--skip-crds"
else
  HTTP_CRDS_FLAG="--set crds.create=true"
  HTTP_SKIP_FLAG=""
fi

# Adopt cluster-scoped RBAC to current release to avoid Helm "ownership" errors.
adopt_http_cluster_scoped "${REL_HTTP}" "${NS}"

echo "[*] Installing/Upgrading KEDA HTTP add-on in namespace '${NS}'"
helm upgrade --install "${REL_HTTP}" kedacore/keda-add-ons-http \
  ${HTTP_VER:+--version "$HTTP_VER"} \
  -n "${NS}" \
  --set operator.keda.enabled=false \
  --set operator.watchNamespace="${NS}" \
  --set fullnameOverride="${REL_HTTP}" \
  ${HTTP_CRDS_FLAG} \
  ${HTTP_SKIP_FLAG} \
  -f values-timeout.yaml \
  --timeout 900s --atomic --debug

# Ensure per-namespace ClusterRoleBindings exist for this namespace (idempotent).
echo "[*] Ensuring per-namespace ClusterRoleBindings for '${NS}'"
ensure_http_namespace_crbs "${NS}"

# ----------------------------- wait & status ---------------------------------

echo "[*] Waiting for deployments in ${NS} to become Available"
kubectl wait --for=condition=Available deploy -n "${NS}" --all --timeout=600s || true

echo "[*] Pods in ${NS}:"
kubectl get pods -n "${NS}" -o wide || true

echo
echo "[✓] Done. KEDA Core (once per cluster) + HTTP add-on (per namespace) are configured."
echo "Notes:"
echo "  - CRDs are cluster-scoped (single owner). The script adopts CRD/RBAC to the current release to avoid Helm ownership errors."
echo "  - Per-namespace ClusterRoleBindings are applied so each namespace SAs have the required cluster permissions."
echo "  - Timeout is extended (--timeout 900s --atomic) to tolerate image pulls and scheduling."
