#!/usr/bin/env bash
set -euo pipefail

# ------------------------------------------------------------------------------
# KEDA Core (once-per-cluster) + KEDA HTTP add-on (per-namespace) installer
#
# עושה:
#  - יוצר/מוודא Namespace
#  - מוסיף/מעדכן Helm repos
#  - מזהה אם CRDs של KEDA Core קיימים (cluster-scoped)
#    * אם חסרים: מתקין Core פעם אחת ויוצר CRDs (בעלים של ה-CRDs)
#      - watchNamespace="" כדי שיפקח על כל הניימספייסים
#      - מכבה CloudEvents/Eventing כדי לא ליצור CRDs נוספים מיותרים
#    * אם קיימים: מדלג על Core
#  - מתקין את KEDA HTTP add-on לכל סביבה (Namespace) בנפרד
#  - מאמץ מראש (adopt) את כל משאבי ה-HTTP add-on ה-cluster-scoped ל-Helm release
#    כדי להימנע משגיאות ownership בהתקנות נוספות
#
# שימוש:
#   ./keda.sh <namespace> [keda_chart_version] [http_addon_chart_version]
#
# דוגמאות:
#   ./keda.sh featest
#   ./keda.sh testkeda 2.17.2 0.11.0
#
# הערות:
#  - CRDs הם cluster-scoped וקיימים פעם אחת לקלאסטר.
#  - ההתקנה הראשונה יוצרת אותם; הבאות מדלגות.
# ------------------------------------------------------------------------------

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <namespace> [keda_chart_version] [http_addon_chart_version]"
  exit 1
fi

NS="$1"
KEDA_VER="${2:-}"   # optional: pin kedacore/keda chart version (e.g., 2.17.2)
HTTP_VER="${3:-}"   # optional: pin kedacore/keda-add-ons-http chart version (e.g., 0.11.0)

REL_CORE="keda-${NS}"        # used רק אם זו ההתקנה הראשונה (Core)
REL_HTTP="keda-http-${NS}"   # release per namespace for HTTP add-on

command -v kubectl >/dev/null 2>&1 || { echo "kubectl not found"; exit 1; }
command -v helm    >/dev/null 2>&1 || { echo "helm not found"; exit 1; }

echo "[*] Namespace: ${NS}"
kubectl get ns "${NS}" >/dev/null 2>&1 || kubectl create ns "${NS}"

echo "[*] Helm repos"
helm repo add kedacore https://kedacore.github.io/charts >/dev/null 2>&1 || true
helm repo update >/dev/null 2>&1 || true

# ----------------------------- helpers ---------------------------------------

# 0 אם כל ה-CRDs קיימים; אחרת 1
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

# אימוץ CRD לבעלות ה-Helm release (מונע שגיאות ownership)
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

# אימוץ כל ה-RBAC ה-cluster-scoped של ה-HTTP add-on לשחרור הנוכחי
adopt_http_cluster_scoped() {
  local rel="$1" ns="$2"

  # אימוץ ClusterRoles/ClusterRoleBindings ששמם מתחיל ב-keda-add-ons-http-
  for kind in clusterrole clusterrolebinding; do
    kubectl get "$kind" -o name | grep -E '/keda-add-ons-http-' || true | while read -r obj; do
      [[ -z "${obj}" ]] && continue
      echo "[*] Adopting ${obj} -> ${rel}/${ns}"
      kubectl annotate "${obj}" \
        "meta.helm.sh/release-name=${rel}" \
        "meta.helm.sh/release-namespace=${ns}" --overwrite || true
      kubectl label "${obj}" \
        "app.kubernetes.io/managed-by=Helm" --overwrite || true
    done
  done
}

# ----------------------------- CRDs/Core -------------------------------------

CORE_CRDS=( \
  scaledobjects.keda.sh \
  scaledjobs.keda.sh \
  triggerauthentications.keda.sh \
  clustertriggerauthentications.keda.sh \
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

# אם ה-CRD כבר קיים, אימץ אותו ל-release של הניימספייס הזה כדי למנוע שגיאות בעלות
if kubectl get crd "${HTTP_CRD}" >/dev/null 2>&1; then
  adopt_crd_to_release "${HTTP_CRD}" "${REL_HTTP}" "${NS}"
  HTTP_CRDS_FLAG="--set crds.create=false"
  HTTP_SKIP_FLAG="--skip-crds"
else
  HTTP_CRDS_FLAG="--set crds.create=true"
  HTTP_SKIP_FLAG=""
fi

# אימוץ RBAC cluster-scoped של ה-HTTP add-on לפני התקנה (מונע ownership errors)
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

# ----------------------------- wait & status ---------------------------------

echo "[*] Waiting for deployments in ${NS} to become Available"
kubectl wait --for=condition=Available deploy -n "${NS}" --all --timeout=600s || true

echo "[*] Pods in ${NS}:"
kubectl get pods -n "${NS}" -o wide || true

echo
echo "[✓] Done. KEDA Core (once per cluster) + HTTP add-on (per namespace) are configured."
echo "Notes:"
echo "  - CRDs are cluster-scoped (single owner). The script adopts CRD/RBAC to the current release to avoid Helm ownership errors."
echo "  - Timeout extended (--timeout 900s --atomic) to avoid context deadline exceeded while images pull or pods schedule."
