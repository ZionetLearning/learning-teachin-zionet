#!/bin/bash

set -e

K8S_NAMESPACE="devops-model"

echo "WARNING: This will delete all resources in namespace '$K8S_NAMESPACE' and Dapr control plane. Continue? (y/N)"
read -r confirm
if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
  echo "Aborted."
  exit 0
fi

echo "Deleting all workloads, services, and configs in $K8S_NAMESPACE..."
kubectl delete all --all -n $K8S_NAMESPACE || true
kubectl delete secret --all -n $K8S_NAMESPACE || true
kubectl delete configmap --all -n $K8S_NAMESPACE || true
kubectl delete component.dapr.io --all -n $K8S_NAMESPACE || true
kubectl delete configuration.dapr.io --all -n $K8S_NAMESPACE || true
kubectl delete pvc --all -n $K8S_NAMESPACE || true

echo "Deleting the namespace $K8S_NAMESPACE..."
kubectl delete namespace $K8S_NAMESPACE || true

echo "Uninstalling Dapr from the cluster..."
dapr uninstall -k --all

echo "Removing Dapr CRDs (in case dapr uninstall misses them)..."
kubectl delete crd components.dapr.io configurations.dapr.io subscriptions.dapr.io serviceinvitations.dapr.io resiliencies.dapr.io bindings.dapr.io || true

echo "All local resources deleted!"
