#!/usr/bin/env bash
set -euo pipefail


helm repo add kedacore https://kedacore.github.io/charts
helm repo update

echo "[*] Installing/Upgrading KEDA HTTP add-on"
helm upgrade --install keda-add-ons-http kedacore/keda-add-ons-http \
  --namespace "keda" \
  --version "0.11.1" \
  -f values-timeout.yaml



echo "[*] KEDA HTTP add-on installation/upgrade complete"
