#!/bin/bash

# Bash script to completely remove Jaeger
# Run this to safely delete all Jaeger components including ServiceMonitor

echo "🗑️ Removing Jaeger observability setup..."
echo ""

# First, explicitly delete ServiceMonitor (in case it exists outside the namespace)
echo "🔍 Checking for Jaeger ServiceMonitor..."
if kubectl get servicemonitor jaeger-metrics -n observability >/dev/null 2>&1; then
    echo "   Found ServiceMonitor 'jaeger-metrics' - deleting..."
    if kubectl delete servicemonitor jaeger-metrics -n observability; then
        echo "   ✅ ServiceMonitor deleted successfully"
    else
        echo "   ⚠️ Failed to delete ServiceMonitor (may not affect cleanup)"
    fi
else
    echo "   No ServiceMonitor found"
fi
echo ""

# Check if namespace exists
if kubectl get namespace observability >/dev/null 2>&1; then
    echo "📋 Current Jaeger resources:"
    kubectl get all -n observability
    echo ""
    
    # Also check for ServiceMonitors in the namespace
    if kubectl get servicemonitor -n observability >/dev/null 2>&1; then
        echo "📊 ServiceMonitors in observability namespace:"
        kubectl get servicemonitor -n observability
        echo ""
    fi
    
    echo "🗑️ Deleting observability namespace and all resources..."
    if kubectl delete namespace observability; then
        echo ""
        echo "✅ Jaeger namespace deleted!"
        
        # Wait and verify
        echo "⏳ Waiting for namespace cleanup..."
        sleep 5
        if ! kubectl get namespace observability >/dev/null 2>&1; then
            echo "✅ Namespace completely removed"
        else
            echo "⏳ Namespace deletion in progress (this may take a few moments)..."
        fi
    else
        echo "❌ Failed to delete namespace"
        exit 1
    fi
else
    echo "ℹ️ No observability namespace found"
fi

# Clean up Dapr tracing configuration
echo ""
echo "🔗 Removing Dapr tracing configuration..."
if kubectl get configuration tracing-config -n dapr-system >/dev/null 2>&1; then
    if kubectl delete configuration tracing-config -n dapr-system; then
        echo "✅ Dapr tracing configuration removed"
    else
        echo "❌ Failed to remove Dapr tracing config"
    fi
else
    echo "ℹ️ No Dapr tracing configuration found"
fi

echo ""
echo "📊 Verification - Your existing services remain unaffected:"
kubectl get namespaces | grep -E "(dapr-system|devops-logs|monitoring|dev|prod|kube-system)" || echo "No matching namespaces found"

echo ""
echo "🎯 Cleanup complete!"
echo ""
echo "📝 What was removed:"
echo "   • Jaeger All-in-One deployment"
echo "   • Jaeger Service (UI, collector, admin ports)"
echo "   • Jaeger ServiceMonitor (Prometheus integration)"
echo "   • Jaeger Ingress (web access)"
echo "   • Observability namespace"
echo "   • Dapr tracing configuration"
echo ""
echo "💡 To reinstall: ./jaeger-install.sh"
