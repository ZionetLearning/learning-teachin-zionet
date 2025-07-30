#!/bin/bash

set -e

K8S_DIR="./k8s for local with rabbitmq and cosmosdb-emulator/k8s"
NAMESPACE_MODEL="$K8S_DIR/namespace-model.yaml"
NAMESPACE_MONITORING="$K8S_DIR/namespace-monitoring.yaml"

echo "Checking prerequisites..."
command -v docker >/dev/null 2>&1 || { echo "Docker not found. Aborting."; exit 1; }
command -v kubectl >/dev/null 2>&1 || { echo "kubectl not found. Aborting."; exit 1; }

echo "Docker and kubectl are available."

# set to local kubcetl context
kubectl config use-context docker-desktop

# Step 1: Create namespaces
echo "Creating namespaces..."
kubectl apply -f "$NAMESPACE_MODEL"
kubectl apply -f "$NAMESPACE_MONITORING"


# Step 2: Install Dapr (if not installed)
if ! dapr --version >/dev/null 2>&1; then
  echo "Installing Dapr CLI..."
  wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash
fi

echo "Initializing Dapr on Kubernetes..."
dapr init -k

# === WAIT FOR DAPR-SYSTEM READY  ===
for i in {1..60}; do
  POD=$(kubectl get pods -n dapr-system -l app=dapr-sidecar-injector -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
  if [[ -n "$POD" ]]; then
    READY=$(kubectl get pod -n dapr-system "$POD" -o jsonpath='{.status.containerStatuses[0].ready}' 2>/dev/null)
    if [[ "$READY" == "true" ]]; then
      echo "Dapr sidecar injector ($POD) is READY!"
      break
    else
      echo "Waiting for Dapr sidecar injector ($POD) to be ready..."
    fi
  else
    echo "Waiting for Dapr sidecar injector pod to be created..."
  fi
  sleep 2
done
if [[ "$READY" != "true" ]]; then
  echo "Timeout waiting for Dapr sidecar injector to be ready."
  exit 1
fi

# Step 3: Apply secrets
echo "Applying secrets..."
kubectl apply -f "$K8S_DIR/secrets" --recursive

# Step 4: Apply Dapr components
echo "Applying Dapr components..."
kubectl apply -f "$K8S_DIR/dapr" --recursive

# Step 5: Apply services
echo "Applying services..."
kubectl apply -f "$K8S_DIR/services" --recursive

# Step 6: Apply deployments
export DOCKER_REGISTRY="benny902" # or read from env/.env file
echo "Applying deployments with registry substitution..."
find "$K8S_DIR/deployments" -type f -name '*.yaml' | while read file; do
  echo "Applying $file"
  envsubst < "$file" | kubectl apply -f -
done

# Step 7: Install Prometheus + Grafana stack (optimized for Docker Desktop)
echo "Adding Prometheus Helm repo..."
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

echo "Installing kube-prometheus-stack..."
helm install monitoring-stack prometheus-community/kube-prometheus-stack \
  --namespace monitoring \
  --create-namespace \
  --set prometheus.prometheusSpec.podMonitorSelectorNilUsesHelmValues=false \
  --set prometheus.prometheusSpec.serviceMonitorSelectorNilUsesHelmValues=false \
  --set grafana.adminPassword="prom-operator" \
  --set grafana.service.type=NodePort \
  --set prometheus.service.type=NodePort \
  --set kubeEtcd.enabled=false \
  --set kubeScheduler.enabled=false \
  --set kubeControllerManager.enabled=false \
  --set kubeProxy.enabled=false \
  --set nodeExporter.enabled=false


  # Step 8: Port forward Grafana and Prometheus for local access

echo "Waiting for Grafana and Prometheus pods to be ready..."

for i in {1..60}; do
  GRAFANA_READY=$(kubectl get pod -n monitoring -l "app.kubernetes.io/name=grafana,app.kubernetes.io/instance=monitoring-stack" -o jsonpath='{.items[0].status.phase}' 2>/dev/null)
  PROM_READY=$(kubectl get pod -n monitoring -l "app.kubernetes.io/name=prometheus,app.kubernetes.io/instance=monitoring-stack" -o jsonpath='{.items[0].status.phase}' 2>/dev/null)

  if [[ "$GRAFANA_READY" == "Running" && "$PROM_READY" == "Running" ]]; then
    echo "Grafana and Prometheus pods are running."
    break
  else
    echo "Waiting... Grafana=$GRAFANA_READY, Prometheus=$PROM_READY"
    sleep 5
  fi
done

if [[ "$GRAFANA_READY" != "Running" || "$PROM_READY" != "Running" ]]; then
  echo "Timeout waiting for pods. Please check pod status manually."
  kubectl get pods -n monitoring
  exit 1
fi

echo "Port forwarding Grafana (localhost:3000) and Prometheus (localhost:9090)..."
kubectl port-forward svc/monitoring-stack-grafana 3000:80 -n monitoring &
kubectl port-forward svc/monitoring-stack-kube-prom-prometheus 9090:9090 -n monitoring &



echo "All resources applied successfully!"
