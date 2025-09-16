#!/bin/bash

# Deploy Langfuse - AI observability platform
set -euo pipefail

NAMESPACE="devops-tools"
echo "🎯 Deploying Langfuse into fixed namespace: $NAMESPACE"

helm repo add langfuse https://langfuse.github.io/langfuse-k8s || true
helm repo update

kubectl create namespace "$NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

if helm list -n "$NAMESPACE" | grep -q "langfuse"; then
    echo "📊 Upgrading existing Langfuse installation..."
    ACTION="upgrade"
else
    echo "📊 Installing Langfuse..."
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
  --set postgresql.auth.database="langfuse-$NAMESPACE" \
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
  --set langfuse.web.startupProbe.enabled=true \
  --set langfuse.web.startupProbe.initialDelaySeconds=30 \
  --set langfuse.web.startupProbe.periodSeconds=10 \
  --set langfuse.web.startupProbe.timeoutSeconds=10 \
  --set langfuse.web.startupProbe.failureThreshold=30 \
  --set langfuse.web.livenessProbe.initialDelaySeconds=60 \
  --set langfuse.web.livenessProbe.timeoutSeconds=10 \
  --set langfuse.web.readinessProbe.initialDelaySeconds=60 \
  --set langfuse.web.readinessProbe.timeoutSeconds=10 \
  --wait \
  --timeout=15m

echo "✅ Langfuse deployed successfully!"
kubectl wait --for=condition=Ready pod -l app.kubernetes.io/name=langfuse -n "$NAMESPACE" --timeout=600s
