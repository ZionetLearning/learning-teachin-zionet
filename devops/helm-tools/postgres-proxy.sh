#!/bin/bash
set -e

# Namespace for the shared proxy
PROXY_NAMESPACE="db-proxy"

# Create namespace if it doesn't exist
kubectl create namespace "$PROXY_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -


# Deploy/upgrade the proxy using a standalone Kubernetes manifest
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MANIFEST_PATH="$SCRIPT_DIR/../kubernetes/proxy/postgres-proxy.yaml"

# Substitute the FQDN in the manifest and apply
if [ -z "$POSTGRES_PRIVATE_FQDN" ]; then
  echo "❌ POSTGRES_PRIVATE_FQDN environment variable is not set."
  exit 1
fi

sed "s|\${POSTGRES_PRIVATE_FQDN}|$POSTGRES_PRIVATE_FQDN|g" "$MANIFEST_PATH" | kubectl apply -f -

echo "✅ postgres-proxy deployed in namespace $PROXY_NAMESPACE"
