#!/usr/bin/env bash
for i in {1..30}; do
  if kubectl get pods -n dapr-system | grep sidecar-injector; then
    exit 0
  fi
  echo "Waiting for Dapr control plane to be available..."
  sleep 2
done
echo "Timeout waiting for Dapr control plane"
exit 1
