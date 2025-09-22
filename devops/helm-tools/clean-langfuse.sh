#!/bin/bash
set -euo pipefail

NAMESPACE="devops-tools"

echo "🧹 Cleaning up Langfuse from namespace: $NAMESPACE"

# 1. Uninstall Helm release
echo "🚮 Uninstalling Helm release..."
helm uninstall langfuse -n "$NAMESPACE" --keep-history=false || true

# 2. Delete migration job
echo "🚮 Deleting migration job..."
kubectl delete job langfuse-migrate -n "$NAMESPACE" --ignore-not-found

# #### if this is deleted a full cicd (terraform apply) is needed, so uncomment only if you want to 'full cicd destroy'
# # 3. Delete ExternalSecret + synced Secret
# echo "🚮 Deleting secrets..."
# kubectl delete externalsecret langfuse-secrets -n "$NAMESPACE" --ignore-not-found
# kubectl delete secret langfuse-secrets -n "$NAMESPACE" --ignore-not-found

# 4. Delete PVCs (ClickHouse, Redis, S3, Zookeeper)
echo "🚮 Deleting PVCs..."
kubectl delete pvc --all -n "$NAMESPACE" --ignore-not-found=true

# 5. Delete leftover deployments/statefulsets/pods (if any)
echo "🚮 Deleting deployments, statefulsets, pods..."
kubectl delete deploy,sts,po -l app.kubernetes.io/instance=langfuse -n "$NAMESPACE" --ignore-not-found

# 6. Delete ingress (optional, comment if you want to keep)
echo "🚮 Deleting ingress..."
kubectl delete ingress langfuse-ingress -n "$NAMESPACE" --ignore-not-found


#kubectl run -n devops-tools temp-delete-all-users --image=postgres:16 --rm -i --restart=Never -- psql "host=dev-pg-zionet-learning.postgres.database.azure.com port=5432 dbname=langfuse-lang user=postgres password=postgres sslmode=require" -c "DELETE FROM users;"

echo "✅ Cleanup complete."

# 7. Show what's left
kubectl get all -n "$NAMESPACE" || true
kubectl get pvc -n "$NAMESPACE" || true
kubectl get externalsecret,secret -n "$NAMESPACE" | grep langfuse || true