#!/bin/bash
set -e

# Namespace for the shared proxy
PROXY_NAMESPACE="db-proxy"

# Create namespace if it doesn't exist
kubectl create namespace "$PROXY_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -

# Deploy/upgrade the proxy using Helm with inline values (no values.yaml dependency)
REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null || echo "$PWD/../../..")"
CHART_PATH="$REPO_ROOT/devops/kubernetes/charts"
helm upgrade --install postgres-proxy "$CHART_PATH" \
  --namespace "$PROXY_NAMESPACE" \
  --set namespace.name="$PROXY_NAMESPACE" \
  --set namespace.create=false \
  --set postgresProxy.enabled=true \
  --set postgresProxy.name="postgres-proxy" \
  --set postgresProxy.labels.selectorKey="app" \
  --set postgresProxy.labels.selectorValue="postgres-proxy" \
  --set postgresProxy.image.name="alpine/socat" \
  --set postgresProxy.image.tag="latest" \
  --set postgresProxy.image.pullPolicy="IfNotPresent" \
  --set postgresProxy.ports.container=5432 \
  --set postgresProxy.ports.service=5432 \
  --set postgresProxy.service.type="ClusterIP" \
  --set postgresProxy.target.host="$POSTGRES_PRIVATE_FQDN" \
  --set postgresProxy.target.port=5432 \
  --set postgresProxy.resources.requests.cpu="50m" \
  --set postgresProxy.resources.requests.memory="64Mi" \
  --set postgresProxy.resources.limits.cpu="100m" \
  --set postgresProxy.resources.limits.memory="128Mi" \
  --set postgresProxy.probes.liveness.initialDelaySeconds=30 \
  --set postgresProxy.probes.liveness.periodSeconds=10 \
  --set postgresProxy.probes.readiness.initialDelaySeconds=10 \
  --set postgresProxy.probes.readiness.periodSeconds=5

echo "✅ postgres-proxy deployed in namespace $PROXY_NAMESPACE"
