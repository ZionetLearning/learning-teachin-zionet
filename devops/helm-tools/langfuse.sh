#!/bin/bash
set -euo pipefail

NAMESPACE="devops-tools"
ENVIRONMENT_NAME="${1:-dev}"

echo "🎯 Deploying Langfuse into $NAMESPACE (DB suffix: $ENVIRONMENT_NAME)"

helm repo add langfuse https://langfuse.github.io/langfuse-k8s || true
helm repo update

kubectl create namespace "$NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

ACTION="install"
if helm status langfuse -n "$NAMESPACE" >/dev/null 2>&1; then
  ACTION="upgrade"
else
  helm uninstall langfuse -n "$NAMESPACE" --keep-history=false || true
  kubectl delete pvc --all -n "$NAMESPACE" --ignore-not-found=true
  sleep 5
fi

# Phase 1: install with web replicas=0 (avoid race with migrations)
helm $ACTION langfuse langfuse/langfuse \
  --namespace "$NAMESPACE" \
  --set langfuse.replicas=0 \
  --set langfuse.nextauth.url="https://teachin.westeurope.cloudapp.azure.com/langfuse" \
  --set langfuse.salt.secretKeyRef.name="langfuse-secrets" \
  --set langfuse.salt.secretKeyRef.key="SALT" \
  --set langfuse.nextauth.secret.secretKeyRef.name="langfuse-secrets" \
  --set langfuse.nextauth.secret.secretKeyRef.key="NEXTAUTH_SECRET" \
  --set langfuse.resources.requests.cpu="250m" \
  --set langfuse.resources.requests.memory="512Mi" \
  --set langfuse.resources.limits.cpu="1000m" \
  --set langfuse.resources.limits.memory="1Gi" \
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
  --set postgresql.auth.secretKeys.usernameKey="DATABASE_USERNAME" \
  --set postgresql.auth.secretKeys.userPasswordKey="DATABASE_PASSWORD" \
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
  --set s3.auth.existingSecret="langfuse-secrets" \
  --set s3.auth.rootUserSecretKey="S3_USER" \
  --set s3.auth.rootPasswordSecretKey="S3_PASSWORD" \
  --set s3.bucket="langfuse-bucket" \
  --set langfuse.worker.replicas=1 \
  --set langfuse.worker.resources.requests.cpu="50m" \
  --set langfuse.worker.resources.requests.memory="128Mi" \
  --set langfuse.worker.resources.limits.cpu="200m" \
  --set langfuse.worker.resources.limits.memory="256Mi" \
  --set langfuse.additionalEnv[0].name="LANGFUSE_LOG_LEVEL" \
  --set-string langfuse.additionalEnv[0].value="debug" \
  --set langfuse.additionalEnv[1].name="LANGFUSE_AUTO_POSTGRES_MIGRATION_DISABLED" \
  --set-string langfuse.additionalEnv[1].value="true" \
  --timeout=5m

echo "✅ Chart applied with web=0. Running Prisma migrations as a Job..."

# --- Phase 1.5: run Prisma migrations once, with the same secrets env as the app ---
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
        args: ["npx prisma migrate deploy --schema=packages/shared/prisma/schema.prisma"]
        envFrom:
        - secretRef:
            name: langfuse-secrets
EOF

kubectl wait --for=condition=complete job/langfuse-migrate -n "$NAMESPACE" --timeout=300s
kubectl delete job langfuse-migrate -n "$NAMESPACE" --ignore-not-found

echo "✅ Migrations applied."

# --- Phase 2: scale web back up to actually serve traffic ---
helm upgrade langfuse langfuse/langfuse \
  --namespace "$NAMESPACE" \
  --set langfuse.replicas=1 \
  --reuse-values

kubectl rollout status deploy/langfuse-web -n "$NAMESPACE" --timeout=300s
kubectl rollout status deploy/langfuse-worker -n "$NAMESPACE" --timeout=300s

echo "🎉 Langfuse deployed successfully."
