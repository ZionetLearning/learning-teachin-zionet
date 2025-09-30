#!/usr/bin/env bash
set -euo pipefail

# ------------------------------------------------------------------------------
# KEDA + KEDA HTTP add-on per-environment installer (namespaced)
#
# What this script does:
#  - Ensures namespace exists
#  - Ensures Helm repos exist
#  - Detects whether KEDA/Core and HTTP add-on CRDs already exist cluster-wide
#  - Installs/Upgrades KEDA Core and HTTP add-on in THE GIVEN NAMESPACE ONLY
#  - Uses fullnameOverride so resource names are unique per environment
#  - Skips CRD creation when CRDs are already present to avoid Helm ownership clashes
#
# Usage:
#   ./keda.sh <namespace> [keda_chart_version] [http_addon_chart_version]
#
# Examples:
#   ./keda.sh featest
#   ./keda.sh testkeda 2.17.2 0.10.0
#
# Notes:
#  - CRDs are cluster-scoped and must exist only once in the cluster.
#  - The first environment that runs this script will CREATE the CRDs.
#  - Subsequent environments will SKIP CRD creation (avoids Helm annotation conflicts).
#  - Pass explicit chart versions if you want to pin them.
# ------------------------------------------------------------------------------

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <namespace> [keda_chart_version] [http_addon_chart_version]"
  exit 1
fi

NS="$1"
KEDA_VER="${2:-}"      # optional Helm chart version for kedacore/keda
HTTP_VER="${3:-}"      # optional Helm chart version for kedacore/keda-add-ons-http

# Release names are unique per namespace to avoid RBAC/name collisions
REL_CORE="keda-${NS}"
REL_HTTP="keda-http-${NS}"

# Ensure tools exist
command -v kubectl >/dev/null 2>&1 || { echo "kubectl not found"; exit 1; }
command -v helm    >/dev/null 2>&1 || { echo "helm not found"; exit 1; }

echo "[*] Namespace: ${NS}"
kubectl get ns "${NS}" >/dev/null 2>&1 || kubectl create ns "${NS}"

echo "[*] Helm repos"
helm repo add kedacore https://kedacore.github.io/charts >/dev/null 2>&1 || true
helm repo update >/dev/null 2>&1 || true

# ------------------------------------------------------------------------------
# Helper: detect whether a list of CRDs exists (all of them)
# Returns 0 if ALL are present, 1 otherwise.
# ------------------------------------------------------------------------------
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

# ------------------------------------------------------------------------------
# Detect CRDs presence and decide flags:
#  - If CRDs are missing: install WITH CRDs (no --skip-crds, set crds.create=true)
#  - If CRDs exist     : install WITHOUT CRDs (--skip-crds, set crds.create=false)
# ------------------------------------------------------------------------------

# KEDA Core CRDs
CORE_CRDS=( \
  scaledobjects.keda.sh \
  scaledjobs.keda.sh \
  triggerauthentications.keda.sh \
  clustertriggerauthentications.keda.sh \
)

if all_crds_exist "${CORE_CRDS[@]}"; then
  CORE_CRDS_FLAG="--set crds.create=false"
  CORE_SKIP_FLAG="--skip-crds"
  echo "[*] KEDA Core CRDs exist (cluster-scoped) -> will skip creating CRDs"
else
  CORE_CRDS_FLAG="--set crds.create=true"
  CORE_SKIP_FLAG=""
  echo "[*] KEDA Core CRDs missing -> will create CRDs in this run"
fi

# HTTP add-on CRD
if kubectl get crd httpscaledobjects.http.keda.sh >/dev/null 2>&1; then
  HTTP_CRDS_FLAG="--set crds.create=false"
  HTTP_SKIP_FLAG="--skip-crds"
  echo "[*] KEDA HTTP CRD exists -> will skip creating CRDs"
else
  HTTP_CRDS_FLAG="--set crds.create=true"
  HTTP_SKIP_FLAG=""
  echo "[*] KEDA HTTP CRD missing -> will create CRD in this run"
fi

# ------------------------------------------------------------------------------
# Install/Upgrade KEDA Core (namespaced scope)
# - watchNamespace limits operator scope to this namespace
# - fullnameOverride ensures all resource names are unique per env
# ------------------------------------------------------------------------------
echo "[*] Installing/Upgrading KEDA Core in namespace '${NS}'"
helm upgrade --install "${REL_CORE}" kedacore/keda \
  ${KEDA_VER:+--version "$KEDA_VER"} \
  -n "${NS}" \
  --set watchNamespace="${NS}" \
  --set fullnameOverride="${REL_CORE}" \
  ${CORE_CRDS_FLAG} \
  ${CORE_SKIP_FLAG} \
  --set cloudEvents.enabled=false \
  --set cloudevents.enabled=false \
  --set eventing.enabled=false \
  --wait --timeout 300s

# ------------------------------------------------------------------------------
# Install/Upgrade KEDA HTTP Add-on (namespaced scope)
# - operator.keda.enabled=false as Core is installed separately
# - operator.watchNamespace limits add-on operator to this namespace
# - fullnameOverride ensures all resource names are unique per env
# ------------------------------------------------------------------------------
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
  --wait --timeout 300s

# ------------------------------------------------------------------------------
# Wait until everything in the namespace is ready (best-effort)
# ------------------------------------------------------------------------------
echo "[*] Waiting for deployments in ${NS} to become Available"
kubectl wait --for=condition=Available deploy -n "${NS}" --all --timeout=300s || true

echo "[*] Pods in ${NS}:"
kubectl get pods -n "${NS}"

echo "[✓] KEDA Core + HTTP add-on are ready in namespace '${NS}'"
echo
echo "Tips:"
echo "  - If you ever see 'CRD is terminating' errors, remove finalizers from the CRD:"
echo "      kubectl patch crd <name> -p '{\"metadata\":{\"finalizers\":[]}}' --type=merge"
echo "  - If you ever see Helm ownership errors for CRDs, DO NOT try to recreate CRDs;"
echo "    install with '--skip-crds' and 'crds.create=false' (this script already does that automatically)."