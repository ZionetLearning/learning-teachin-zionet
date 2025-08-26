#!/bin/bash

# Bash script to completely remove Jaeger
# Run this to safely delete all Jaeger components

echo "Removing Jaeger observability setup..."
echo ""

# Check if namespace exists
if kubectl get namespace observability >/dev/null 2>&1; then
    echo "📋 Current Jaeger resources:"
    kubectl get all -n observability
    echo ""
    
    echo "Deleting observability namespace and all resources..."
    if kubectl delete namespace observability; then
        echo ""
        echo "Jaeger namespace deleted!"
        
        # Wait and verify
        sleep 3
        if ! kubectl get namespace observability >/dev/null 2>&1; then
            echo "Namespace completely removed"
        else
            echo "Namespace deletion in progress..."
        fi
    else
        echo "Failed to delete namespace"
        exit 1
    fi
else
    echo "No observability namespace found"
fi

# Clean up Dapr tracing configuration
echo ""
echo "🔗 Removing Dapr tracing configuration..."
if kubectl get configuration tracing-config -n dapr-system >/dev/null 2>&1; then
    if kubectl delete configuration tracing-config -n dapr-system; then
        echo "Dapr tracing configuration removed"
    else
        echo "Failed to remove Dapr tracing config"
    fi
else
    echo "No Dapr tracing configuration found"
fi

echo ""
echo "Your existing services remain unaffected:"
kubectl get namespaces | grep -E "(dapr-system|devops-logs|monitoring|dev|prod)"

echo ""
echo "Cleanup complete!"
echo "To reinstall: ./jaeger-install.sh"
