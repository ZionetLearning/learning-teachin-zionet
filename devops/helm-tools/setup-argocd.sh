#!/bin/bash
set -euo pipefail

ARGOCD_NAMESPACE="argocd"
HELM_CHART_VERSION="9.1.4" # last version 9.1.4

# create namespace if not exists
echo "📦 Creating namespace '${ARGOCD_NAMESPACE}'..."
kubectl create namespace ${ARGOCD_NAMESPACE} --dry-run=client -o yaml | kubectl apply -f -
echo ""

# add ArgoCD Helm repo
echo "📚 Adding ArgoCD Helm repository..."
helm repo add argo https://argoproj.github.io/argo-helm
helm repo update
echo ""

# install ArgoCD with Helm
echo "⚙️  Installing ArgoCD..."
helm upgrade --install argocd argo/argo-cd \
  --namespace ${ARGOCD_NAMESPACE} \
  --version ${HELM_CHART_VERSION} \
  --create-namespace \
  --wait \
  --timeout 10m


# get initial admin password
echo "🔑 Getting initial admin password..."
sleep 5  
ARGOCD_PASSWORD=$(kubectl -n ${ARGOCD_NAMESPACE} get secret argocd-initial-admin-secret \
  -o jsonpath="{.data.password}" 2>/dev/null | base64 -d || echo "")

if [ -z "$ARGOCD_PASSWORD" ]; then
    echo "⏳ Waiting for password secret to be created..."
    kubectl wait --for=condition=ready pod -l app.kubernetes.io/name=argocd-server -n ${ARGOCD_NAMESPACE} --timeout=300s
    sleep 5
    ARGOCD_PASSWORD=$(kubectl -n ${ARGOCD_NAMESPACE} get secret argocd-initial-admin-secret \
      -o jsonpath="{.data.password}" | base64 -d)
fi



# retrieve initial admin password
echo ""
echo "======================================"
echo "✅ ArgoCD Helm installation complete!"
echo "======================================"
echo ""
echo "📋 Access Information:"
echo "------------------------------------"
echo "Username: admin"
echo "Password: ${ARGOCD_PASSWORD}"
echo ""
echo "🌐 To access ArgoCD UI:"
echo "1. Run: kubectl port-forward svc/argocd-server -n argocd 8080:443"
echo "2. Open: https://localhost:8080"
echo ""
echo "💡 Password saved to: .argocd-password"
echo "======================================"
echo ""