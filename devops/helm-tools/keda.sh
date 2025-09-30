#!/usr/bin/env bash
set -euo pipefail

# ------------------------------------------------------------------------------
# KEDA Core (once-per-cluster) + KEDA HTTP add-on (per-namespace) installer
#
# What this script does:
#  - Ensures a target namespace exists
#  - Ensures Helm repos exist
#  - Detects whether KEDA Core CRDs already exist (cluster-scoped)
#  - If CRDs are missing: installs KEDA Core ONCE and creates CRDs (cluster owner)
#    * The first Core install is set to watchNamespace="" so it serves all namespaces
#    * CloudEvents is disabled to avoid extra CRDs/ownership complexity
#  - If CRDs exist: SKIPS Core install and only installs the HTTP add-on in the given namespace
#  - Uses fullnameOverride so resource names are unique per environment
#
# Usage:
#   ./keda.sh <namespace> [keda_chart_version] [http_addon_chart_version]
#
# Examples:
#   ./keda.sh featest
#   ./keda.sh testkeda 2.17.2 0.10.0
#
# Notes:
#  - CRDs are cluster-scoped and must be created ONCE per cluster.
#  - First environment that runs this script becomes the CRD owner.
#  - Subsequent environments will skip Core and only install the HTTP add-on.
# ------------------------------------------------------------------------------

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <namespace> [keda_chart_version] [http_addon_chart_version]"
  exit 1
fi

NS="$1"
KEDA_VER="${2:-}"   # optional: pin kedacore/keda chart version (e.g., 2.17.2)
HTTP_VER="${3:-}"   # optional: pin kedacore/keda-add-ons-http chart version (e.g., 0.10.0)

REL_CORE="keda-${NS}"        # release name if this is the FIRST (core) install
REL_HTTP="keda-http-${NS}"   # per-namespace HTTP add-on release

command -v kubectl >/dev/null 2>&1 || { echo "kubectl not found"; exit 1; }
command -v helm    >/dev/null 2>&1 || { echo "helm not found"; exit 1; }

echo "[*] Namespace: ${NS}"
kubectl get ns "${NS}" >/dev/null 2>&1 || kubectl create ns "${NS}"

echo "[*] Helm repos"
helm repo add kedacore https://kedacore.github.io/charts >/dev/null 2>&1 || true
helm repo update >/dev/null 2>&1 || true

# ------------------------------------------------------------------------------
# Helper: return 0 if ALL given CRDs exist, 1 otherwise
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

# KEDA Core CRDs that indicate Core was initialized cluster-wide
CORE_CRDS=( \
  scaledobjects.keda.sh \
  scaledjobs.keda.sh \
  triggerauthentications.keda.sh \
  clustertriggerauthentications.keda.sh \
)

HTTP_CRD=httpscaledobjects.http.keda.sh

# ------------------------------------------------------------------------------
# Decide: first install (create CRDs + Core) OR subsequent env (skip Core)
# ------------------------------------------------------------------------------
FIRST_CORE_INSTALL=0
if all_crds_exist "${CORE_CRDS[@]}"; then
  echo "[*] KEDA Core CRDs already exist -> Core is already installed in this cluster"
else
  echo "[*] KEDA Core CRDs are missing -> this will be the FIRST Core install (cluster owner)"
  FIRST_CORE_INSTALL=1
fi

# ------------------------------------------------------------------------------
# FIRST Core install (once per cluster): create CRDs and install Core (cluster-wide)
# ------------------------------------------------------------------------------
if [[ "${FIRST_CORE_INSTALL}" -eq 1 ]]; then
  echo "[*] Installing KEDA Core (first install, cluster-wide owner of CRDs)"

  # We set watchNamespace="" so the operator watches all namespaces.
  # We disable CloudEvents to avoid extra CRDs (and ownership issues).
  helm upgrade --install "${REL_CORE}" kedacore/keda \
    ${KEDA_VER:+--version "$KEDA_VER"} \
    -n "${NS}" \
    --set watchNamespace="" \
    --set fullnameOverride="${REL_CORE}" \
    --set crds.create=true \
    --wait --timeout 300s \
    --set cloudEvents.enabled=false \
    --set cloudevents.enabled=false \
    --set eventing.enabled=false

  # Ensure the HTTP CRD exists as well (it may be created by the add-on, but we check now)
  if ! kubectl get crd "${HTTP_CRD}" >/dev/null 2>&1; then
    echo "[*] HTTP CRD (${HTTP_CRD}) not present yet -> will be created by HTTP add-on install"
  fi

else
  echo "[*] Skipping KEDA Core install in '${NS}' (CRDs already exist; Core managed by the first install)"
fi

# ------------------------------------------------------------------------------
# Always install/upgrade KEDA HTTP add-on per namespace (namespaced operator)
# ------------------------------------------------------------------------------
echo "[*] Installing/Upgrading KEDA HTTP add-on in namespace '${NS}'"

# If the HTTP CRD exists, avoid creating/owning it again (skip CRDs).
HTTP_CRDS_FLAG="--set crds.create=false"
HTTP_SKIP_FLAG="--skip-crds"
if ! kubectl get crd "${HTTP_CRD}" >/dev/null 2>&1; then
  HTTP_CRDS_FLAG="--set crds.create=true"
  HTTP_SKIP_FLAG=""
fi

helm upgrade --install "${REL_HTTP}" kedacore/keda-add-ons-http \
  ${HTTP_VER:+--version "$HTTP_VER"} \
  -n "${NS}" \
  --set operator.keda.enabled=false \
  --set operator.watchNamespace="${NS}" \
  --set fullnameOverride="${REL_HTTP}" \
  ${HTTP_CRDS_FLAG} \
  ${HTTP_SKIP_FLAG} \
  --wait --timeout 300s

# ------------------------------------------------------------------------------
# Wait for readiness (best-effort) and show status
# ------------------------------------------------------------------------------
echo "[*] Waiting for deployments in ${NS} to become Available"
kubectl wait --for=condition=Available deploy -n "${NS}" --all --timeout=300s || true

echo "[*] Pods in ${NS}:"
kubectl get pods -n "${NS}" || true

echo
echo "[✓] Done. KEDA Core (once) + HTTP add-on (per-namespace) are configured."
echo "Tips:"
echo "  - First run creates CRDs and installs Core (cluster-wide)."
echo "  - Subsequent runs SKIP Core and only install HTTP add-on in the target namespace."
echo "  - If you ever see a CRD 'Terminating' error, remove finalizers:"
echo "      kubectl patch crd <name> -p '{\"metadata\":{\"finalizers\":[]}}' --type=merge"
