#!/usr/bin/env bash
set -euo pipefail

########################################
# Configuration
########################################
ENVIRONMENT="${1:-Development}"

# Azure / AKS
AKS_RG="dev-zionet-learning-2025-keda"
AKS_NAME="aks-cluster-dev"
MC_AKS_RG="MC_${AKS_RG}_${AKS_NAME}_westeurope"

# Docker / OpenAI
DOCKER_REGISTRY="${DOCKER_USERNAME:-mydockerhub}"
OPENAI_KEY="${AZURE_OPENAI_API_KEY:-}"

# Kubernetes paths
BASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
K8S_DIR="$BASE_DIR/kubernetes"
NAMESPACE_FILE="$K8S_DIR/namespaces/namespace-dev.yaml"

# Helm scripts directory
HELM_DIR="$BASE_DIR/helm-tools"

# PostgreSQL
PG_DB="pg-zionet-dev-2025-keda"

########################################
# Step 2: Get AKS credentials
########################################
echo "Fetching AKS credentials..."
az aks get-credentials --resource-group "$AKS_RG" --name "$AKS_NAME" --overwrite-existing

########################################
# Step 3: Install/Upgrade Dapr
########################################
echo "Installing Dapr control plane..."
chmod +x "$HELM_DIR/dapr-control-plane.sh"
"$HELM_DIR/dapr-control-plane.sh"

########################################
# Step 4: Create namespace
########################################
echo "Creating namespace..."
kubectl apply -f "$NAMESPACE_FILE"

########################################
# Step 5: Apply Dapr components
########################################
echo "Applying Dapr components..."
kubectl apply -f "$K8S_DIR/dapr/components" --recursive

########################################
# Step 6: Apply services
########################################
echo "Applying Kubernetes services..."
kubectl apply -f "$K8S_DIR/services" --recursive

########################################
# Step 7: Apply deployments with env substitution
########################################
echo "Applying deployments..."
find "$K8S_DIR/deployments" -type f -name '*.yaml' | while read -r file; do
    echo "Applying $file..."
    DOCKER_REGISTRY="$DOCKER_REGISTRY" OPENAI_KEY="$OPENAI_KEY" envsubst < "$file" | kubectl apply -f -
done

########################################
# Step 8: Setup ingress controller
########################################
echo "Setting up ingress controller..."
chmod +x "$HELM_DIR/setup-ingress-controller.sh"
"$HELM_DIR/setup-ingress-controller.sh"

########################################
# Step 9: Install KEDA
########################################
echo "Installing KEDA..."
chmod +x "$HELM_DIR/keda.sh"
"$HELM_DIR/keda.sh"

echo "Applying KEDA components..."
kubectl apply -f "$K8S_DIR/keda" --recursive

########################################
# Step 10: Install Grafana
########################################
echo "Installing Grafana..."
chmod +x "$HELM_DIR/grafana.sh"
"$HELM_DIR/grafana.sh"

########################################
# Step 11: Setup HTTPS certificates
########################################
echo "Setting up HTTPS certificates..."
chmod +x "$HELM_DIR/setup-https.sh"
"$HELM_DIR/setup-https.sh"

########################################
# Step 12: Apply ingress resources
########################################
echo "Applying ingress resources..."
kubectl apply -f "$K8S_DIR/ingress" --recursive

########################################
# Step 13: Install Prometheus
########################################
echo "Installing Prometheus..."
chmod +x "$HELM_DIR/prometheus/prometheus.sh"
"$HELM_DIR/prometheus/prometheus.sh"

########################################
# Step 14: Install Loki (logs)
########################################
echo "Installing Loki..."
chmod +x "$HELM_DIR/loki/loki.sh"
"$HELM_DIR/loki/loki.sh"

########################################
# Step 15: Wait for external IP for ingress
########################################
echo "Waiting for ingress external IP..."
for i in {1..30}; do
    INGRESS_IP=$(kubectl -n devops-ingress-nginx get svc ingress-nginx-controller -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
    if [[ -n "$INGRESS_IP" ]]; then
        echo "Ingress IP ready: $INGRESS_IP"
        break
    fi
    echo "Attempt $i: Ingress IP not assigned yet. Waiting 10s..."
    sleep 10
done

if [[ -z "$INGRESS_IP" ]]; then
    echo "Failed to get ingress IP. Exiting."
    exit 1
fi

########################################
# Step 16: Add ingress IP to PostgreSQL firewall
########################################
echo "Adding ingress IP to PostgreSQL firewall..."
az postgres flexible-server firewall-rule create \
    --resource-group "$AKS_RG" \
    --name "$PG_DB" \
    --rule-name allow-ingress-controller \
    --start-ip-address "$INGRESS_IP" \
    --end-ip-address "$INGRESS_IP"

########################################
# Step 17: Restart all pods in dev namespace
########################################
echo "Restarting all pods in dev namespace..."
kubectl delete pod --all -n dev

echo "Deployment complete!"
echo "App URL: http://$INGRESS_IP"