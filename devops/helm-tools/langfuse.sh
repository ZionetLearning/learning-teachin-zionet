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
  --set postgresql.deploy=false \
  --set postgresql.host="dev-pg-zionet-learning.postgres.database.azure.com" \
  --set postgresql.port=5432 \
  --set postgresql.auth.existingSecret="langfuse-secrets" \
  --set clickhouse.auth.existingSecret="langfuse-secrets" \
  --set clickhouse.auth.existingSecretKey="CLICKHOUSE_PASSWORD" \
  --set clickhouse.resources.requests.cpu="100m" \
  --set clickhouse.resources.requests.memory="256Mi" \
  --set clickhouse.resources.limits.cpu="200m" \
  --set clickhouse.resources.limits.memory="512Mi" \
  --set clickhouse.zookeeper.resources.requests.cpu="50m" \
  --set clickhouse.zookeeper.resources.requests.memory="128Mi" \
  --set clickhouse.zookeeper.resources.limits.cpu="100m" \
  --set clickhouse.zookeeper.resources.limits.memory="256Mi" \
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

echo "✅ Chart installed with web=0. Running migrations..."

# Run migration job
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

echo "✅ Migrations applied. Scaling web up..."

# Phase 2: scale web back to 1 replica
helm upgrade langfuse langfuse/langfuse \
  --namespace "$NAMESPACE" \
  --set langfuse.replicas=1 \
  --reuse-values

kubectl rollout status deploy/langfuse-web -n "$NAMESPACE" --timeout=300s
kubectl rollout status deploy/langfuse-worker -n "$NAMESPACE" --timeout=300s

echo "🎉 Langfuse deployed successfully."
