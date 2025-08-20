#!/bin/bash

# Quick cleanup script for temporary environments
# Usage: ./cleanup-temp-envs.sh [dry-run|force] [environment-pattern]

set -e

MODE=${1:-"dry-run"}
PATTERN=${2:-"*"}

# Protected environments - NEVER delete these
PROTECTED_ENVS="dev,prod,production,staging"

echo "🧹 Temporary Environment Cleanup"
echo "Mode: $MODE"
echo "Pattern: $PATTERN"
echo "Protected: $PROTECTED_ENVS"
echo ""

if [ "$MODE" != "dry-run" ] && [ "$MODE" != "force" ]; then
    echo "❌ Invalid mode. Use 'dry-run' or 'force'"
    echo "Usage: $0 [dry-run|force] [pattern]"
    exit 1
fi

# Get AKS credentials
echo "🔗 Getting AKS credentials..."
az aks get-credentials --resource-group dev-zionet-learning-2025 --name aks-cluster-dev --overwrite-existing > /dev/null

# Find resource groups to cleanup
echo "🔍 Finding temporary resource groups..."
TEMP_RGS=$(az group list --query "[?contains(name, 'zionet-learning-2025')].name" -o tsv | grep -v "^dev-\|^prod-\|^production-\|^staging-" || true)

# Find namespaces to cleanup  
echo "🔍 Finding temporary namespaces..."
TEMP_NAMESPACES=$(kubectl get namespaces --no-headers | grep -v "kube-\|default\|devops-\|^dev\s\|^prod\s\|^production\s\|^staging\s" | awk '{print $1}' || true)

echo ""
echo "📋 Found temporary resources:"
echo "Resource Groups:"
for rg in $TEMP_RGS; do
    ENV_NAME=$(echo "$rg" | cut -d'-' -f1)
    if [[ ",$PROTECTED_ENVS," != *",$ENV_NAME,"* ]]; then
        echo "  ✓ $rg (env: $ENV_NAME)"
    else
        echo "  ❌ $rg (PROTECTED - skipping)"
        TEMP_RGS=$(echo "$TEMP_RGS" | grep -v "^$rg$")
    fi
done

echo "Namespaces:"
for ns in $TEMP_NAMESPACES; do
    if [[ ",$PROTECTED_ENVS," != *",$ns,"* ]]; then
        AGE=$(kubectl get namespace "$ns" --no-headers 2>/dev/null | awk '{print $3}' || echo "unknown")
        echo "  ✓ $ns (age: $AGE)"
    else
        echo "  ❌ $ns (PROTECTED - skipping)"
        TEMP_NAMESPACES=$(echo "$TEMP_NAMESPACES" | grep -v "^$ns$")
    fi
done

if [ "$MODE" = "dry-run" ]; then
    echo ""
    echo "🏃 DRY RUN - No actual deletions will be performed"
    echo ""
    echo "To actually delete these resources, run:"
    echo "  $0 force"
    exit 0
fi

# Confirmation for force mode
echo ""
read -p "⚠️  Are you sure you want to DELETE these resources? Type 'yes' to confirm: " confirm
if [ "$confirm" != "yes" ]; then
    echo "❌ Cancelled"
    exit 1
fi

# Delete namespaces
echo ""
echo "🗑️ Deleting temporary namespaces..."
for ns in $TEMP_NAMESPACES; do
    echo "Deleting namespace: $ns"
    kubectl delete namespace "$ns" --timeout=300s || echo "Failed to delete $ns"
done

# Delete resource groups
echo ""
echo "🗑️ Deleting temporary resource groups..."
for rg in $TEMP_RGS; do
    echo "Deleting resource group: $rg (background)"
    az group delete --name "$rg" --yes --no-wait || echo "Failed to delete $rg"
done

echo ""
echo "✅ Cleanup initiated!"
echo "📝 Resource groups are being deleted in the background"
echo "🔍 Check Azure portal for deletion progress"
