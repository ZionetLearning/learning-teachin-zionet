#!/bin/bash
set -e

NAMESPACE="devops-logs"
EMAIL="snir1552@gmail.com"
DOMAIN="teachin-zionet.westeurope.cloudapp.azure.com"

echo "1. Installing cert-manager..."
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

echo "2. Waiting for cert-manager to be ready..."
kubectl wait --for=condition=ready pod -l app=cert-manager -n cert-manager --timeout=120s

echo "3. Creating Let's Encrypt ClusterIssuer..."
kubectl apply -f ../kubernetes/ingress/letsencrypt-clusterissuer.yaml

echo "4. Applying the HTTPS-enabled grafana ingress..."
kubectl apply -f ../kubernetes/ingress/grafana-ingress.yaml

echo "5. Waiting for certificate to be issued..."
echo "This may take 1-2 minutes..."
kubectl wait --for=condition=ready certificate teachin-zionet-tls -n $NAMESPACE --timeout=300s

echo "HTTPS setup complete!"
echo "Your secure Grafana URL: https://$DOMAIN/grafana/"
echo ""
echo "Check certificate status with:"
echo "kubectl get certificate -n $NAMESPACE"
echo "kubectl describe certificate teachin-zionet-tls -n $NAMESPACE"
