#!/bin/bash
set -euo pipefail

NAMESPACE="prod"  # Changed from "devops-logs" to "prod"
EMAIL="snir1552@gmail.com"
DOMAIN="teachinprod.westeurope.cloudapp.azure.com"

echo "1. Installing cert-manager..."
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

echo "2) Wait for cert-manager deployments to be Available"
kubectl -n cert-manager rollout status deploy/cert-manager --timeout=3m
kubectl -n cert-manager rollout status deploy/cert-manager-cainjector --timeout=3m
kubectl -n cert-manager rollout status deploy/cert-manager-webhook --timeout=3m

echo "2.1) Ensure webhook service has endpoints"
kubectl -n cert-manager wait --for=jsonpath='{.subsets[0].addresses[0].ip}' endpoints/cert-manager-webhook --timeout=120s

sleep 30

echo "3) Create/Update Let's Encrypt ClusterIssuer (retry until webhook is up)"
for i in {1..5}; do
  if kubectl apply -f ../kubernetes/ingress/letsencrypt-clusterissuer.yaml; then
    break
  fi
  echo "ClusterIssuer apply failed (try $i). Waiting 10s and retrying…"
  sleep 10
done

# echo "4. Applying the HTTPS-enabled grafana ingress..."
# kubectl apply -f ../kubernetes/ingress/grafana-ingress.yaml

echo "5. Waiting for certificate to be issued..."
echo "This may take 1-2 minutes..."
kubectl wait --for=condition=ready certificate teachin-tls -n $NAMESPACE --timeout=300s

echo "HTTPS setup complete!"
echo "Your secure Grafana URL: https://$DOMAIN/grafana/"
echo ""
echo "Check certificate status with:"
echo "kubectl get certificate -n $NAMESPACE"
echo "kubectl describe certificate teachin-tls -n $NAMESPACE"
