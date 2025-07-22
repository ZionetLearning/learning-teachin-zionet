#!/usr/bin/env bash
for i in {1..60}; do
  POD=$(kubectl get pods -n dapr-system -l app=dapr-sidecar-injector -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
  if [[ -n "$POD" ]]; then
    READY=$(kubectl get pod -n dapr-system "$POD" -o jsonpath='{.status.containerStatuses[0].ready}' 2>/dev/null)
    if [[ "$READY" == "true" ]]; then
      echo "✅ Dapr sidecar injector ($POD) is READY!"
      exit 0
    else
      echo "Waiting for Dapr sidecar injector ($POD) to be ready..."
    fi
  else
    echo "Waiting for Dapr sidecar injector pod to be created..."
  fi
  sleep 2
done
echo "❌ Timeout waiting for Dapr sidecar injector to be ready."
exit 1
