#!/usr/bin/env bash
set -euo pipefail

echo "[1/8] Uninstall Helm releases whose names start with keda- / keda-http- (all namespaces)"
for ns in $(kubectl get ns -o jsonpath='{.items[*].metadata.name}'); do
  for rel in $(helm -n "$ns" ls -q | grep -E '^(keda|keda-http)-' || true); do
    helm -n "$ns" uninstall "$rel" || true
  done
done

echo "[2/8] Delete namespaced KEDA HTTP add-on resources by label"
kubectl delete deploy,svc,sa,role,rolebinding \
  -A -l app.kubernetes.io/part-of=keda-add-ons-http --ignore-not-found || true

echo "[3/8] Delete cluster-scoped RBAC (HTTP add-on & variants)"
kubectl delete clusterrole,clusterrolebinding \
  keda-add-ons-http-interceptor \
  keda-add-ons-http-external-scaler \
  keda-add-ons-http-role \
  keda-add-ons-http-proxy-role \
  keda-add-ons-http-metrics-reader \
  --ignore-not-found || true
kubectl delete clusterrole,clusterrolebinding \
  -l app.kubernetes.io/part-of=keda-add-ons-http --ignore-not-found || true

echo "[4/8] Delete KEDA objects cluster-wide (instances) if any"
kubectl get httpscaledobjects -A -o name 2>/dev/null | xargs -r kubectl delete --ignore-not-found || true
kubectl get scaledobjects -A -o name 2>/dev/null | xargs -r kubectl delete --ignore-not-found || true
kubectl get scaledjobs -A -o name 2>/dev/null | xargs -r kubectl delete --ignore-not-found || true
kubectl get triggerauthentications -A -o name 2>/dev/null | xargs -r kubectl delete --ignore-not-found || true
kubectl get clustertriggerauthentications -A -o name 2>/dev/null | xargs -r kubectl delete --ignore-not-found || true

echo "[5/8] Drop finalizers on any lingering instances (best-effort)"
for kind in httpscaledobjects scaledobjects scaledjobs triggerauthentications clustertriggerauthentications; do
  for obj in $(kubectl get "$kind" -A -o name 2>/dev/null || true); do
    kubectl patch "$obj" --type=merge -p '{"metadata":{"finalizers":[]}}' || true
  done
done

echo "[6/8] Delete CRDs and force-remove finalizers if they hang"
CRDS=(
  httpscaledobjects.http.keda.sh
  scaledobjects.keda.sh
  scaledjobs.keda.sh
  triggerauthentications.keda.sh
  clustertriggerauthentications.keda.sh
)

for crd in "${CRDS[@]}"; do
  kubectl delete crd "$crd" --ignore-not-found || true
  if kubectl get crd "$crd" >/dev/null 2>&1; then
    echo "  - Force-clearing finalizers on CRD $crd"
    kubectl get crd "$crd" -o json \
      | sed 's/"finalizers":[[^]]*]/"finalizers":[]/g' \
      | kubectl replace -f - || true
    kubectl delete crd "$crd" --ignore-not-found || true
  fi
done

echo "[7/8] Remove KEDA-related webhooks and aggregated APIServices (can hold deletion)"
# Webhooks carrying keda labels or names
kubectl get validatingwebhookconfiguration,mutatingwebhookconfiguration -o name 2>/dev/null \
  | grep -i keda || true \
  | xargs -r kubectl delete --ignore-not-found || true
kubectl delete validatingwebhookconfiguration,mutatingwebhookconfiguration \
  -l app=keda --ignore-not-found || true

# External/Custom metrics APIService – delete if it points to KEDA’s adapter
for api in v1beta1.external.metrics.k8s.io v1beta1.custom.metrics.k8s.io; do
  if kubectl get apiservice "$api" >/dev/null 2>&1; then
    # Check if it references the KEDA metrics service
    if kubectl get apiservice "$api" -o jsonpath='{.spec.service.name}' 2>/dev/null \
      | grep -qi 'keda-operator-metrics-apiserver'; then
      kubectl delete apiservice "$api" --ignore-not-found || true
    fi
  fi
done

echo "[8/8] Show any leftovers (should be empty)"
kubectl get crd | egrep -i 'keda|http.keda' || echo "No KEDA CRDs remain."
echo "[✓] KEDA cleanup complete."
