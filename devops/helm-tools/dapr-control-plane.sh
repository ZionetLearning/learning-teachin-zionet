#!/bin/bash
set -e


helm repo add dapr https://dapr.github.io/helm-charts || true
helm repo update
kubectl get ns dapr-system >/dev/null 2>&1 || kubectl create ns dapr-system
helm upgrade --install dapr dapr/dapr \
  --version "1.15.9" \
  --namespace "dapr-system" \
  --create-namespace \
  --set global.daprComponentInitTimeout="120s" \
  --wait