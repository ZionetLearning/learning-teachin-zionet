#!/bin/bash
set -e

# Namespaces
GRAFANA_NAMESPACE="devops-logs"

# Validate environment variables
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

echo "1. Check if Grafana is running in the '$GRAFANA_NAMESPACE' namespace"
kubectl wait --namespace "$GRAFANA_NAMESPACE" \
  --for=condition=Ready pod \
  --selector=app.kubernetes.io/name=grafana \
  --timeout=120s

echo "2. Create Kubernetes Secret for Azure credentials"
kubectl create secret generic azure-monitor-secrets -n $GRAFANA_NAMESPACE \
  --from-literal=AZURE_TENANT_ID=$AZURE_TENANT_ID \
  --from-literal=AZURE_SUBSCRIPTION_ID=$AZURE_SUBSCRIPTION_ID \
  --from-literal=AZURE_CLIENT_ID=$AZURE_CLIENT_ID \
  --from-literal=AZURE_CLIENT_SECRET=$AZURE_CLIENT_SECRET \
  --dry-run=client -o yaml | kubectl apply -f -

echo "3. Create ConfigMap for Azure Monitor datasource"
cat <<EOF | kubectl apply -f -
apiVersion: v1
kind: ConfigMap
metadata:
  name: grafana-datasource-azure-monitor
  namespace: $GRAFANA_NAMESPACE
  labels:
    grafana_datasource: "1"
data:
  azure-monitor-datasource.yaml: |
    apiVersion: 1
    datasources:
      - name: Azure Monitor
        type: grafana-azure-monitor-datasource
        uid: azure-monitor
        access: proxy
        editable: true
        isDefault: false
        jsonData:
          cloudName: azuremonitor
          tenantId: $AZURE_TENANT_ID
          subscriptionId: $AZURE_SUBSCRIPTION_ID
          clientId: $AZURE_CLIENT_ID
          defaultSubscription: $AZURE_SUBSCRIPTION_ID
        secureJsonData:
          clientSecret: $AZURE_CLIENT_SECRET
        version: 1
EOF

echo "4. Update Grafana deployment to use the Secret"
kubectl patch deployment grafana -n $GRAFANA_NAMESPACE \
  --type='json' \
  -p='[
    {"op": "add", "path": "/spec/template/spec/containers/0/envFrom", "value":[{"secretRef":{"name":"azure-monitor-secrets"}}]}
  ]' || true

echo "5. Create Grafana dashboard ConfigMaps"
for file in dashboards/*.json; do
  DASH_NAME=$(basename "$file" .json)
  kubectl apply -f - <<EOF
apiVersion: v1
kind: ConfigMap
metadata:
  name: dashboard-$DASH_NAME
  namespace: $GRAFANA_NAMESPACE
  labels:
    grafana_dashboard: "1"
data:
  $DASH_NAME.json: |
$(sed 's/^/    /' "$file")
EOF
done

echo "Azure Monitor datasource and dashboards configured successfully."