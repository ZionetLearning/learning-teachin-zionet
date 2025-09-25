#!/bin/bash
set -e

KEDA_CORE_NAMESPACE="keda"
WATCH_NAMESPACES="$1"

if [ -z "$WATCH_NAMESPACES" ]; then
    echo "Error: Please provide a comma-separated list of namespaces for the HTTP interceptor to watch"
    echo "Usage: $0 <namespace1,namespace2,...>"
    exit 1
fi

KEDA_HTTP_NAMESPACE="keda-http"
KEDA_HTTP_RELEASE="keda-http-shared"

echo "=== KEDA Shared HTTP Installation ==="
echo "KEDA Core Namespace: $KEDA_CORE_NAMESPACE"
echo "KEDA HTTP Release:   $KEDA_HTTP_RELEASE"
echo "Namespaces watched:  $WATCH_NAMESPACES"
echo "======================================"

helm repo add kedacore https://kedacore.github.io/charts
helm repo update

kubectl create namespace "$KEDA_CORE_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
if ! helm list -n "$KEDA_CORE_NAMESPACE" | grep -q "^keda\s"; then
  helm upgrade --install keda kedacore/keda \
    --namespace "$KEDA_CORE_NAMESPACE" \
    --wait --timeout 300s
fi
kubectl wait --for=condition=Available deployment \
  -l app.kubernetes.io/name=keda-operator \
  -n "$KEDA_CORE_NAMESPACE" --timeout=300s

kubectl create namespace "$KEDA_HTTP_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
helm upgrade --install "$KEDA_HTTP_RELEASE" kedacore/keda-add-ons-http \
  --namespace "$KEDA_HTTP_NAMESPACE" \
  --set operator.keda.enabled=false \
  --set scaler.kubernetes.kedaNamespace="$KEDA_CORE_NAMESPACE" \
  --set interceptor.kubernetes.watchNamespace="*" \
  --wait --timeout 300s

echo "=== Verification ==="
kubectl get pods -n "$KEDA_CORE_NAMESPACE" -l app.kubernetes.io/name=keda-operator
kubectl get pods -n "$KEDA_HTTP_NAMESPACE" -l app.kubernetes.io/name=keda-add-ons-http
echo "Done!"