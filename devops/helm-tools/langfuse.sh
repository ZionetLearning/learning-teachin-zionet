#!/bin/bash

# Deploy Langfuse - AI observability platform
set -euo pipefail

# Namespace is fixed
NAMESPACE="devops-tools"
# Environment name (used for DB suffix), default to "dev"
ENVIRONMENT_NAME="${1:-dev}"

echo "🎯 Deploying Langfuse into namespace: $NAMESPACE (DB suffix: $ENVIRONMENT_NAME)"

helm repo add langfuse https://langfuse.github.io/langfuse-k8s || true
helm repo update

kubectl create namespace "$NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

RELEASE_STATUS=$(helm status langfuse -n "$NAMESPACE" -o json 2>/dev/null | jq -r '.info.status' || echo "not-found")

if [[ "$RELEASE_STATUS" == "deployed" ]]; then
    echo "📊 Upgrading existing Langfuse installation..."
    ACTION="upgrade"
else
    echo "📊 Installing fresh Langfuse..."
    helm uninstall langfuse -n "$NAMESPACE" --keep-history=false || true
    ACTION="install"
    # clean PVCs on first install to avoid conflicts
    kubectl delete pvc --all -n "$NAMESPACE" --ignore-not-found=true
    sleep 10
fi

helm $ACTION langfuse langfuse/langfuse \
  --namespace "$NAMESPACE" \
  --set langfuse.nextauth.url="https://teachin.westeurope.cloudapp.azure.com/langfuse" \
  --set langfuse.salt.secretKeyRef.name="langfuse-secrets" \
  --set langfuse.salt.secretKeyRef.key="SALT" \
  --set langfuse.nextauth.secret.secretKeyRef.name="langfuse-secrets" \
  --set langfuse.nextauth.secret.secretKeyRef.key="NEXTAUTH_SECRET" \
  --set langfuse.resources.requests.cpu="100m" \
  --set langfuse.resources.requests.memory="256Mi" \
  --set langfuse.resources.limits.cpu="500m" \
  --set langfuse.resources.limits.memory="512Mi" \
  --set langfuse.worker.replicas=1 \
  --set langfuse.worker.resources.requests.cpu="50m" \
  --set langfuse.worker.resources.requests.memory="128Mi" \
  --set langfuse.worker.resources.limits.cpu="200m" \
  --set langfuse.worker.resources.limits.memory="256Mi" \
  --set postgresql.deploy=false \
  --set postgresql.host="dev-pg-zionet-learning.postgres.database.azure.com" \
  --set postgresql.port=5432 \
  --set postgresql.auth.database="langfuse-${ENVIRONMENT_NAME}" \
  --set postgresql.auth.existingSecret="langfuse-secrets" \
  --set postgresql.auth.secretKeys.userPasswordKey="DATABASE_PASSWORD" \
  --set postgresql.auth.username="postgres" \
  --set clickhouse.auth.existingSecret="langfuse-secrets" \
  --set clickhouse.auth.existingSecretKey="CLICKHOUSE_PASSWORD" \
  --set clickhouse.resourcesPreset="nano" \
  --set clickhouse.replicaCount=1 \
  --set clickhouse.resources.requests.cpu="100m" \
  --set clickhouse.resources.requests.memory="256Mi" \
  --set clickhouse.resources.limits.cpu="200m" \
  --set clickhouse.resources.limits.memory="512Mi" \
  --set clickhouse.zookeeper.resources.requests.cpu="50m" \
  --set clickhouse.zookeeper.resources.requests.memory="128Mi" \
  --set clickhouse.zookeeper.resources.limits.cpu="100m" \
  --set clickhouse.zookeeper.resources.limits.memory="256Mi" \
  --set clickhouse.zookeeper.replicaCount=1 \
  --set redis.auth.existingSecret="langfuse-secrets" \
  --set redis.auth.existingSecretPasswordKey="REDIS_PASSWORD" \
  --set redis.primary.resources.requests.cpu="50m" \
  --set redis.primary.resources.requests.memory="64Mi" \
  --set redis.primary.resources.limits.cpu="100m" \
  --set redis.primary.resources.limits.memory="128Mi" \
  --set s3.auth.existingSecret="langfuse-secrets" \
  --set s3.auth.rootUserSecretKey="S3_USER" \
  --set s3.auth.rootPasswordSecretKey="S3_PASSWORD" \
  --set s3.bucket="langfuse-bucket" \
  --set s3.resources.requests.cpu="50m" \
  --set s3.resources.requests.memory="128Mi" \
  --set s3.resources.limits.cpu="100m" \
  --set s3.resources.limits.memory="256Mi" \
  --set langfuse.additionalEnv[0].name="LANGFUSE_LOG_LEVEL" \
  --set-string langfuse.additionalEnv[0].value="debug" \
  --timeout=5m

echo "✅ Helm finished. Now applying migrations..."

# Create migration job
cat <<EOF | kubectl apply -f -
apiVersion: batch/v1
kind: Job
metadata:
  name: langfuse-migrate
  namespace: $NAMESPACE
spec:
  backoffLimit: 1
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: migrate
        image: langfuse/langfuse:3.108.0
        command: ["sh", "-c"]
        args: ["echo '🔄 Running Prisma migrations...'; npx prisma migrate deploy --schema=packages/shared/prisma/schema.prisma"]
        envFrom:
        - secretRef:
            name: langfuse-secrets
EOF

# Wait for migration job to complete
kubectl wait --for=condition=complete job/langfuse-migrate -n "$NAMESPACE" --timeout=300s || {
  echo "❌ Migration job failed, check logs with:"
  echo "kubectl logs job/langfuse-migrate -n $NAMESPACE"
  exit 1
}

# Cleanup migration job (optional)
kubectl delete job langfuse-migrate -n "$NAMESPACE" --ignore-not-found

echo "✅ Migrations applied successfully."

# Rollout checks
kubectl rollout status deploy/langfuse-web -n "$NAMESPACE" --timeout=300s || true
kubectl rollout status deploy/langfuse-worker -n "$NAMESPACE" --timeout=300s || true

echo "🎉 Langfuse deployed and ready."