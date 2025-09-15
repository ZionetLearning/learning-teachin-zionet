#!/bin/bash

# PostgreSQL Private FQDN Fix Script
# This script fixes the PostgreSQL connection string for private networking
# Azure auto-generates DNS records for VNet-integrated PostgreSQL servers

set -euo pipefail

# Configuration - can be overridden by environment variables or parameters
ENVIRONMENT_NAME="${1:-${ENVIRONMENT_NAME:-prod}}"
RESOURCE_GROUP="${2:-${RESOURCE_GROUP:-${ENVIRONMENT_NAME}-zionet-learning-2025}}"
DNS_ZONE="${3:-${DNS_ZONE:-privatelink.postgres.database.azure.com}}"
KEY_VAULT="${4:-${KEY_VAULT:-teachin-seo-kv}}"
SECRET_NAME="${5:-${SECRET_NAME:-${ENVIRONMENT_NAME}-postgres-connection}}"

K8S_SECRET_NAME="${K8S_SECRET_NAME:-postgres-connection}"
K8S_NAMESPACE="${K8S_NAMESPACE:-$ENVIRONMENT_NAME}"


trap 'if [ -n "${GITHUB_OUTPUT:-}" ]; then echo "POSTGRES_PRIVATE_FQDN=" >> "$GITHUB_OUTPUT"; fi' ERR


echo "🔍 Fetching auto-generated PostgreSQL private FQDN..."
echo "📋 Environment: $ENVIRONMENT_NAME"
echo "📋 Resource Group: $RESOURCE_GROUP" 
echo "📋 Key Vault: $KEY_VAULT"
echo "📋 Secret Name: $SECRET_NAME"

# Get the auto-generated DNS record name
PRIVATE_RECORD_NAME=$(az network private-dns record-set a list \
  --zone-name "$DNS_ZONE" \
  --resource-group "$RESOURCE_GROUP" \
  --query '[0].name' \
  --output tsv)

if [ -z "$PRIVATE_RECORD_NAME" ]; then
  echo "❌ Error: No private DNS records found for PostgreSQL"
  exit 1
fi

PRIVATE_FQDN="${PRIVATE_RECORD_NAME}.${DNS_ZONE}"
echo "✅ Found auto-generated FQDN: $PRIVATE_FQDN"

# Get the current connection string
echo "🔄 Updating Key Vault secret..."
CURRENT_CONNECTION=$(az keyvault secret show \
  --vault-name "$KEY_VAULT" \
  --name "$SECRET_NAME" \
  --query 'value' \
  --output tsv)

if [ -z "${CURRENT_CONNECTION}" ]; then
  echo "❌ Error: Secret '$SECRET_NAME' not found or empty in Key Vault '$KEY_VAULT'"
  exit 1
fi


# Replace the host in the connection string
# Extract database, username, password from current string
DATABASE=$(echo "$CURRENT_CONNECTION" | grep -o 'Database=[^;]*' | cut -d'=' -f2 || true)
USERNAME=$(echo "$CURRENT_CONNECTION" | grep -o 'Username=[^;]*' | cut -d'=' -f2 || true)
PASSWORD=$(echo "$CURRENT_CONNECTION" | grep -o 'Password=[^;]*' | cut -d'=' -f2 || true)
SSL_MODE=$(echo "$CURRENT_CONNECTION" | grep -o 'SslMode=[^;]*' | cut -d'=' -f2 || true)

if [ -z "${SSL_MODE}" ]; then
  SSL_MODE="Require"
fi


# Build new connection string with correct private FQDN
NEW_CONNECTION="Host=${PRIVATE_FQDN};Database=${DATABASE};Username=${USERNAME};Password=${PASSWORD};SslMode=${SSL_MODE}"

# Update Key Vault secret
az keyvault secret set \
  --vault-name "$KEY_VAULT" \
  --name "$SECRET_NAME" \
  --value "$NEW_CONNECTION" \
  --output none

echo "✅ Updated Key Vault secret with private FQDN"


# Force External Secrets to refresh (if in Kubernetes environment and context is set)
if command -v kubectl &>/dev/null && kubectl config current-context &>/dev/null; then
  echo "🔄 Refreshing Kubernetes secret '${K8S_SECRET_NAME}' in namespace '${K8S_NAMESPACE}'..."
  kubectl delete secret "${K8S_SECRET_NAME}" -n "${K8S_NAMESPACE}" --ignore-not-found=true

  echo "⏳ Waiting for External Secrets to sync..."
  sleep 10

  if kubectl get secret "${K8S_SECRET_NAME}" -n "${K8S_NAMESPACE}" &>/dev/null; then
    echo "✅ Kubernetes secret refreshed successfully"
  else
    echo "⚠️  Warning: Kubernetes secret not found yet - External Secrets may need more time to sync"
  fi
fi

if [ -n "${GITHUB_OUTPUT:-}" ]; then
  echo "POSTGRES_PRIVATE_FQDN=$PRIVATE_FQDN" >> "$GITHUB_OUTPUT"
fi

echo "🎉 PostgreSQL private FQDN fix completed!"
echo "📝 Private FQDN: $PRIVATE_FQDN"
echo "🔑 Key Vault secret updated: $SECRET_NAME"
