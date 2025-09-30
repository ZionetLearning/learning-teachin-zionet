#!/bin/bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "usage: $0 <namespace> [keda_version] [http_addon_version]"
  echo "ex:    $0 featest 2.14.0 0.10.0"
  exit 1
fi

NS="$1"
KEDA_VER="${2:-}"     
HTTP_VER="${3:-}"        

REL_CORE="keda-${NS}"
REL_HTTP="keda-http-${NS}"

echo "[*] namespace: ${NS}"
kubectl get ns "${NS}" >/dev/null 2>&1 || kubectl create ns "${NS}"

echo "[*] helm repos"
helm repo add kedacore https://kedacore.github.io/charts >/dev/null
helm repo update >/dev/null

echo "[*] ensure CRDs exist"
# CRDs של KEDA Core
if ! kubectl get crd clustertriggerauthentications.keda.sh >/dev/null 2>&1; then
  echo "    installing KEDA core (to install CRDs first time)"
  helm upgrade --install "${REL_CORE}" kedacore/keda \
    ${KEDA_VER:+--version "$KEDA_VER"} \
    -n "${NS}" \
    --set watchNamespace="${NS}" \
    --wait --timeout 300s
else
  echo "    CRDs exist; installing/upgrading namespaced core without CRDs"
  helm upgrade --install "${REL_CORE}" kedacore/keda \
    ${KEDA_VER:+--version "$KEDA_VER"} \
    -n "${NS}" \
    --set watchNamespace="${NS}" \
    --wait --timeout 300s \
    --skip-crds
fi

# CRDs HTTP Add-on (HTTPScaledObject)
if ! kubectl get crd httpscaledobjects.http.keda.sh >/dev/null 2>&1; then
  echo "    installing HTTP add-on (to install CRDs first time)"
  helm upgrade --install "${REL_HTTP}" kedacore/keda-add-ons-http \
    ${HTTP_VER:+--version "$HTTP_VER"} \
    -n "${NS}" \
    --set operator.keda.enabled=false \
    --set operator.watchNamespace="${NS}" \
    --set fullnameOverride="${REL_HTTP}" \
    --wait --timeout 300s
else
  echo "    CRD httpscaledobjects exists; installing/upgrading add-on without CRDs"
  helm upgrade --install "${REL_HTTP}" kedacore/keda-add-ons-http \
    ${HTTP_VER:+--version "$HTTP_VER"} \
    -n "${NS}" \
    --set operator.keda.enabled=false \
    --set operator.watchNamespace="${NS}" \
    --set fullnameOverride="${REL_HTTP}" \
    --wait --timeout 300s \
    #--skip-crds
fi

echo "[*] waiting for deployments in ${NS}"
kubectl wait --for=condition=Available deploy -n "${NS}" --all --timeout=300s
kubectl get pods -n "${NS}"
echo "[✓] KEDA core + HTTP add-on are ready in namespace ${NS}"