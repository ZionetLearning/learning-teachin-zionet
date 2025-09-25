#!/bin/bash
set -e

KEDA_CORE_NAMESPACE="keda"
KEDA_HTTP_NAMESPACE_TO_INSTALL="$1"
KEDA_HTTP_RELEASE_NAME="keda-http-$RANDOM"

if [ -z "$KEDA_HTTP_NAMESPACE_TO_INSTALL" ]; then
    echo "Error: Please provide the namespace where KEDA HTTP Add-on should be installed"
    echo "Usage: $0 <target-namespace>"
    exit 1
fi

# Function to check if KEDA Core pods are ready
check_keda_core_ready() {
    echo "Checking KEDA Core deployments in namespace: $KEDA_CORE_NAMESPACE"
    if kubectl wait --for=condition=Available deployment --all -n "$KEDA_CORE_NAMESPACE" --timeout=300s --selector=app.kubernetes.io/name=keda-operator 2>/dev/null; then
        echo "KEDA Core deployments are available!"
        kubectl get pods -n "$KEDA_CORE_NAMESPACE" -l app.kubernetes.io/name=keda-operator
        return 0
    else
        echo "KEDA Core deployments are not ready or don't exist"
        return 1
    fi
}

# Update ownership of global KEDA HTTP Add-on resources
update_global_ownership() {
    local namespace="$1"
    local release="$2"

    echo "Updating ownership metadata for global KEDA HTTP Add-on resources..."

    # CRDs
    for crd in httpscaledobjects.http.keda.sh scaledjobs.keda.sh triggerauthentications.keda.sh cloudeventsources.eventing.keda.sh clustertriggerauthentications.keda.sh clustercloudeventsources.eventing.keda.sh; do
        if kubectl get crd "$crd" &>/dev/null; then
            kubectl annotate crd "$crd" meta.helm.sh/release-name="$release" --overwrite
            kubectl annotate crd "$crd" meta.helm.sh/release-namespace="$namespace" --overwrite
            kubectl label crd "$crd" app.kubernetes.io/managed-by=Helm --overwrite || true
        fi
    done

    # ClusterRoles
    for cr in keda-add-ons-http-interceptor keda-add-ons-http-role; do
        if kubectl get clusterrole "$cr" &>/dev/null; then
            kubectl annotate clusterrole "$cr" meta.helm.sh/release-name="$release" --overwrite
            kubectl annotate clusterrole "$cr" meta.helm.sh/release-namespace="$namespace" --overwrite
            kubectl label clusterrole "$cr" app.kubernetes.io/managed-by=Helm --overwrite || true
        fi
    done

    # ClusterRoleBindings
    for crb in keda-add-ons-http-interceptor; do
        if kubectl get clusterrolebinding "$crb" &>/dev/null; then
            kubectl annotate clusterrolebinding "$crb" meta.helm.sh/release-name="$release" --overwrite
            kubectl annotate clusterrolebinding "$crb" meta.helm.sh/release-namespace="$namespace" --overwrite
            kubectl label clusterrolebinding "$crb" app.kubernetes.io/managed-by=Helm --overwrite || true
        fi
    done
}

# Install KEDA HTTP Add-on
install_keda_http() {
    local namespace="$1"
    local release="$2"

    # Ensure namespace exists
    kubectl create namespace "$namespace" --dry-run=client -o yaml | kubectl apply -f -

    # Update ownership for global resources
    update_global_ownership "$namespace" "$release"

    # Install via Helm
    helm upgrade --install "$release" kedacore/keda-add-ons-http \
        --namespace "$namespace" \
        --set operator.keda.enabled=false \
        --set interceptor.kubernetes.watchNamespace="$namespace" \
        --set interceptor.kubernetes.interceptorService.namespaceOverride="$namespace" \
        --set scaler.kubernetes.kedaNamespace="$KEDA_CORE_NAMESPACE" \
        --skip-crds=true \
        -f values-timeout.yaml \
        --wait --timeout 300s

    echo "KEDA HTTP Add-on installed successfully in namespace: $namespace with release name: $release"
}

echo "=== KEDA Installation Script ==="
echo "KEDA Core Namespace: $KEDA_CORE_NAMESPACE"
echo "KEDA HTTP Target Namespace: $KEDA_HTTP_NAMESPACE_TO_INSTALL"
echo "Helm Release Name: $KEDA_HTTP_RELEASE_NAME"
echo "================================="

# Add Helm repo
helm repo add kedacore https://kedacore.github.io/charts || true
helm repo update

# Install or check KEDA Core
if helm list -n "$KEDA_CORE_NAMESPACE" | grep -q "^keda\s"; then
    echo "KEDA Core Helm release found, checking pod status..."
    check_keda_core_ready
else
    echo "KEDA Core not found, installing fresh..."
    kubectl create namespace "$KEDA_CORE_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
    helm upgrade --install keda kedacore/keda \
        --namespace "$KEDA_CORE_NAMESPACE" \
        --wait --timeout 300s
fi

# Install KEDA HTTP Add-on
install_keda_http "$KEDA_HTTP_NAMESPACE_TO_INSTALL" "$KEDA_HTTP_RELEASE_NAME"

# Final verification
echo ""
echo "=== Final Verification ==="
kubectl get pods -n "$KEDA_CORE_NAMESPACE" -l app.kubernetes.io/name=keda-operator
kubectl get pods -n "$KEDA_HTTP_NAMESPACE_TO_INSTALL" -l app.kubernetes.io/name=keda-add-ons-http