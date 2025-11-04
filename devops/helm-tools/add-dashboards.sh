#!/bin/bash
set -e

# ==============================
# Configuration
# ==============================
path=$1
GRAFANA_NAMESPACE="devops-logs"

# ==============================
# Validate path
# ==============================
if [ ! -d "$path" ]; then
  echo "Error: Directory '$path' does not exist"
  exit 1
fi

# ==============================
# Apply Grafana dashboards
# ==============================
for file in $path/*.json; do
  DASH_NAME=$(basename "$file" .json)
  kubectl create configmap "dashboard-$DASH_NAME" \
    --namespace "$GRAFANA_NAMESPACE" \
    --from-file="$DASH_NAME.json=$file" \
    --dry-run=client -o yaml | \
  kubectl label -f - grafana_dashboard="1" --local --dry-run=client -o yaml | \
  kubectl apply -f -
done
