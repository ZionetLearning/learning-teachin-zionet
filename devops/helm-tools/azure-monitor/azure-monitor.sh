#!/bin/bash
set -e

# ==============================
# Configuration
# ==============================
GRAFANA_NAMESPACE="devops-logs"

# ==============================
# 1. Validate Azure environment variables
# ==============================
echo "1. Validating Azure environment variables..."
if [[ -z "$AZURE_TENANT_ID" || -z "$AZURE_SUBSCRIPTION_ID" || -z "$AZURE_CLIENT_ID" || -z "$AZURE_CLIENT_SECRET" ]]; then
  echo "Error: Required Azure environment variables are not set!"
  echo "Make sure GitHub secrets are properly configured:"
  echo "  - AZURE_TENANT_ID"
  echo "  - AZURE_SUBSCRIPTION_ID" 
  echo "  - AZURE_CLIENT_ID"
  echo "  - AZURE_CLIENT_SECRET"
  exit 1
fi

echo "   Azure environment variables are set"
echo "   Tenant ID: ${AZURE_TENANT_ID:0:8}..."
echo "   Subscription ID: ${AZURE_SUBSCRIPTION_ID:0:8}..."
echo "   Client ID: ${AZURE_CLIENT_ID:0:8}..."

# ==============================
# 2. Wait for Grafana pod to be ready
# ==============================
echo "2. Check if Grafana is running in the '$GRAFANA_NAMESPACE' namespace"
kubectl wait --namespace "$GRAFANA_NAMESPACE" \
  --for=condition=Ready pod \
  --selector=app.kubernetes.io/name=grafana \
  --timeout=120s

# ==============================
# 3. Create Azure Monitor datasource and dashboards
# ==============================
echo "3. Create Kubernetes Secret for Azure credentials"
kubectl create secret generic azure-monitor-secrets -n $GRAFANA_NAMESPACE \
  --from-literal=AZURE_TENANT_ID=$AZURE_TENANT_ID \
  --from-literal=AZURE_SUBSCRIPTION_ID=$AZURE_SUBSCRIPTION_ID \
  --from-literal=AZURE_CLIENT_ID=$AZURE_CLIENT_ID \
  --from-literal=AZURE_CLIENT_SECRET=$AZURE_CLIENT_SECRET \
  --dry-run=client -o yaml | kubectl apply -f -

# ==============================
# 4. Create ConfigMap for Azure Monitor datasource
# ==============================
echo "4. Create ConfigMap for Azure Monitor datasource"
envsubst < ./yaml/datasource.yaml | kubectl apply -f -

# ==============================
# 5. Create Grafana dashboard ConfigMaps
# ==============================
echo "5. Create Grafana dashboard ConfigMaps"
bash ../add-dashboards.sh ./dashboards

echo "Azure Monitor datasource and dashboards configured successfully."