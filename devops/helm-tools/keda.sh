#!/usr/bin/env bash
set -euo pipefail

NAMESPACE="keda"
RELEASE_KEDA="keda"
RELEASE_HTTP="keda-add-ons-http"
KEDA_VERSION="2.18.1"
HTTP_ADDON_VERSION="0.11.1"

echo "[*] Checking if KEDA base is installed..."

if helm status "$RELEASE_KEDA" -n "$NAMESPACE" >/dev/null 2>&1; then
    echo "[✓] KEDA base already installed."
else
    echo "[*] KEDA base not found. Installing..."
    helm repo add kedacore https://kedacore.github.io/charts
    helm repo update

    helm upgrade --install "$RELEASE_KEDA" kedacore/keda \
      --namespace "$NAMESPACE" \
      --create-namespace \
      --version "$KEDA_VERSION"
fi

echo ""
echo "[*] Checking if KEDA HTTP add-on is installed..."

if helm status "$RELEASE_HTTP" -n "$NAMESPACE" >/dev/null 2>&1; then
    echo "[✓] KEDA HTTP add-on already installed. Skipping."
else
    echo "[*] Installing KEDA HTTP add-on..."
    helm upgrade --install "$RELEASE_HTTP" kedacore/keda-add-ons-http \
      --namespace "$NAMESPACE" \
      --version "$HTTP_ADDON_VERSION" \
      -f values-timeout.yaml
fi

echo ""
echo "[✓] All KEDA components are installed and ready."