#!/bin/bash

# PostgreSQL Private FQDN Fix Script
# This script fixes the PostgreSQL connection string for private networking
# Azure auto-generates DNS records for VNet-integrated PostgreSQL servers

set -e

# Configuration - can be overridden by environment variables or parameters
ENVIRONMENT_NAME="${1:-${ENVIRONMENT_NAME:-prod}}"
RESOURCE_GROUP="${2:-${RESOURCE_GROUP:-${ENVIRONMENT_NAME}-zionet-learning-2025}}"
DNS_ZONE="${3:-${DNS_ZONE:-privatelink.postgres.database.azure.com}}"
KEY_VAULT="${4:-${KEY_VAULT:-teachin-seo-kv}}"
SECRET_NAME="${5:-${SECRET_NAME:-${ENVIRONMENT_NAME}-postgres-connection}}"

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

# Replace the host in the connection string
# Extract database, username, password from current string
DATABASE=$(echo "$CURRENT_CONNECTION" | grep -o 'Database=[^;]*' | cut -d'=' -f2)
USERNAME=$(echo "$CURRENT_CONNECTION" | grep -o 'Username=[^;]*' | cut -d'=' -f2)
PASSWORD=$(echo "$CURRENT_CONNECTION" | grep -o 'Password=[^;]*' | cut -d'=' -f2)

# Build new connection string with correct private FQDN
NEW_CONNECTION="Host=${PRIVATE_FQDN};Database=${DATABASE};Username=${USERNAME};Password=${PASSWORD};SslMode=Require"

# Update Key Vault secret
az keyvault secret set \
  --vault-name "$KEY_VAULT" \
  --name "$SECRET_NAME" \
  --value "$NEW_CONNECTION" \
  --output none

echo "✅ Updated Key Vault secret with private FQDN"

# Force External Secrets to refresh (if in Kubernetes environment)
if command -v kubectl &> /dev/null; then
  echo "🔄 Refreshing Kubernetes secret..."
  kubectl delete secret postgres-connection -n prod --ignore-not-found=true
  
  # Wait for External Secrets to recreate
  echo "⏳ Waiting for External Secrets to sync..."
  sleep 10
  
  kubectl get secret postgres-connection -n prod &> /dev/null && \
    echo "✅ Kubernetes secret refreshed successfully" || \
    echo "⚠️  Warning: Kubernetes secret not found - External Secrets may need time to sync"
fi

echo "🎉 PostgreSQL private FQDN fix completed!"
echo "📝 Private FQDN: $PRIVATE_FQDN"
echo "🔑 Key Vault secret updated: $SECRET_NAME"
