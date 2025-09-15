#!/bin/bash

# Deploy Langfuse - AI observability platform
# Resource-optimized configuration based on official minimal installation

set -e

echo "🔍 Setting up Langfuse (Resource-Optimized)..."

# Check if we're in dev environment only
NAMESPACE="${TARGET_NAMESPACE:-dev}"
if [[ "$NAMESPACE" != "dev" ]]; then
    echo "❌ Langfuse is only supported for dev environment. Current: $NAMESPACE"
    exit 0
fi

# Add the Langfuse Helm repository
echo "📦 Adding Langfuse Helm repository..."
helm repo add langfuse https://cbeneke.github.io/langfuse-k8s || true
helm repo update

# Check if Langfuse is already installed
if helm list -n devops-tools | grep -q "langfuse"; then
    echo "📊 Upgrading existing Langfuse installation..."
    ACTION="upgrade"
else
    echo "📊 Installing Langfuse..."
    ACTION="install"
fi

# Create namespace if it doesn't exist
kubectl create namespace devops-tools --dry-run=client -o yaml | kubectl apply -f -

# Deploy/upgrade Langfuse with resource-constrained configuration
# All components are required but configured with minimal resources
helm $ACTION langfuse langfuse/langfuse \
    --namespace devops-tools \
    --set langfuse.nextauth.url="https://teachin.westeurope.cloudapp.azure.com/langfuse" \
    --set langfuse.salt.secretKeyRef.name="langfuse-secrets" \
    --set langfuse.salt.secretKeyRef.key="SALT" \
    --set langfuse.nextauth.secret.secretKeyRef.name="langfuse-secrets" \
    --set langfuse.nextauth.secret.secretKeyRef.key="NEXTAUTH_SECRET" \
    --set langfuse.resources.requests.cpu="100m" \
    --set langfuse.resources.requests.memory="256Mi" \
    --set langfuse.resources.limits.cpu="300m" \
    --set langfuse.resources.limits.memory="512Mi" \
    --set langfuse.worker.replicas=0 \
    --set postgresql.deploy=false \
    --set postgresql.host="dev-pg-zionet-learning.postgres.database.azure.com" \
    --set postgresql.port=5432 \
    --set postgresql.auth.database="langfuse-dev" \
    --set postgresql.auth.existingSecret="langfuse-secrets" \
    --set postgresql.auth.secretKeys.userPasswordKey="DATABASE_PASSWORD" \
    --set postgresql.auth.username="zionet_learning" \
    --set clickhouse.auth.existingSecret="langfuse-secrets" \
    --set clickhouse.auth.existingSecretKey="CLICKHOUSE_PASSWORD" \
    --set clickhouse.resourcesPreset="nano" \
    --set clickhouse.replicaCount=1 \
    --set clickhouse.resources.requests.cpu="200m" \
    --set clickhouse.resources.requests.memory="512Mi" \
    --set clickhouse.resources.limits.cpu="400m" \
    --set clickhouse.resources.limits.memory="1Gi" \
    --set clickhouse.zookeeper.resources.requests.cpu="100m" \
    --set clickhouse.zookeeper.resources.requests.memory="256Mi" \
    --set clickhouse.zookeeper.resources.limits.cpu="200m" \
    --set clickhouse.zookeeper.resources.limits.memory="512Mi" \
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
    --wait \
    --timeout=10m

echo "✅ Langfuse deployed successfully!"

# Wait for pods to be ready
echo "⏳ Waiting for Langfuse pods to be ready..."
kubectl wait --for=condition=Ready pod -l app.kubernetes.io/name=langfuse -n devops-tools --timeout=600s

echo "🎉 Langfuse is ready!"
echo "📊 Langfuse will be available at: https://teachin.westeurope.cloudapp.azure.com/langfuse"
