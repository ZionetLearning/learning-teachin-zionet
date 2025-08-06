#!/bin/bash
set -ex


# Define variables
KEDA_NAMESPACE="keda"
KEDA_CORE_VERSION="2.17.2"
KEDA_HTTP_VERSION="0.10.0"


echo "1. Add Helm repo for KEDA"
helm repo add kedacore https://kedacore.github.io/charts || true
helm repo update

echo "2. Create namespace if it doesn't exist"
kubectl get ns "$KEDA_NAMESPACE" >/dev/null 2>&1 || kubectl create ns "$KEDA_NAMESPACE"

echo "3. Install/Upgrade KEDA Core"
helm status keda --namespace "$KEDA_NAMESPACE" >/dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "KEDA Core already installed - upgrading"
  helm upgrade keda kedacore/keda \
    --version "$KEDA_CORE_VERSION" \
    --namespace "$KEDA_NAMESPACE" \
    --wait
else
  echo "Installing KEDA Core"
  helm install keda kedacore/keda \
    --version "$KEDA_CORE_VERSION" \
    --namespace "$KEDA_NAMESPACE" \
    --wait
fi

echo "4. Install/Upgrade KEDA HTTP Add-on"
helm status keda-http --namespace "$KEDA_NAMESPACE" >/dev/null 2>&1
if [ $? -eq 0 ]; then
  echo "KEDA HTTP Add-on already installed - upgrading"
  helm upgrade keda-http kedacore/keda-add-ons-http \
    --version "$KEDA_HTTP_VERSION" \
    --namespace "$KEDA_NAMESPACE" \
    --set operator.keda.enabled=false \
    --wait
else
  echo "Installing KEDA HTTP Add-on"
  helm install keda-http kedacore/keda-add-ons-http \
    --version "$KEDA_HTTP_VERSION" \
    --namespace "$KEDA_NAMESPACE" \
    --set operator.keda.enabled=false \
    --wait
fi

echo "5. Verify KEDA HTTP Add-on is ready"
kubectl wait --namespace "$KEDA_NAMESPACE" \
  --for=condition=Ready pod \
  --selector=app.kubernetes.io/name=keda-http-add-on \
  --timeout=120s

echo "KEDA Core + HTTP Add-on installed and ready"
