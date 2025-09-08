#!/bin/bash

# Script to test Jaeger ServiceMonitor functionality
# Verifies that Prometheus can scrape metrics from Jaeger

echo "🔍 Testing Jaeger ServiceMonitor functionality..."
echo ""

# Check if ServiceMonitor exists
echo "1. Checking ServiceMonitor..."
if kubectl get servicemonitor jaeger-metrics -n observability >/dev/null 2>&1; then
    echo "   ✅ ServiceMonitor 'jaeger-metrics' found"
    kubectl get servicemonitor jaeger-metrics -n observability
else
    echo "   ❌ ServiceMonitor 'jaeger-metrics' not found"
    echo "   Run: kubectl apply -f ../kubernetes/config/jaeger-servicemonitor.yaml"
    exit 1
fi

echo ""

# Check if Jaeger service is running
echo "2. Checking Jaeger service..."
if kubectl get svc jaeger-all-in-one -n observability >/dev/null 2>&1; then
    echo "   ✅ Jaeger service found"
    kubectl get svc jaeger-all-in-one -n observability
else
    echo "   ❌ Jaeger service not found"
    echo "   Run: ./jaeger-install.sh"
    exit 1
fi

echo ""

# Check if Jaeger pod is running
echo "3. Checking Jaeger pod..."
if kubectl get pods -n observability -l app=jaeger --field-selector=status.phase=Running >/dev/null 2>&1; then
    echo "   ✅ Jaeger pod is running"
    kubectl get pods -n observability -l app=jaeger
else
    echo "   ❌ Jaeger pod not running"
    kubectl get pods -n observability -l app=jaeger
    exit 1
fi

echo ""

# Test metrics endpoint
echo "4. Testing metrics endpoint..."
JAEGER_POD=$(kubectl get pods -n observability -l app=jaeger -o jsonpath='{.items[0].metadata.name}')
if [ -n "$JAEGER_POD" ]; then
    echo "   Testing metrics from pod: $JAEGER_POD"
    if kubectl exec -n observability "$JAEGER_POD" -- wget -qO- http://localhost:5778/metrics | head -5; then
        echo ""
        echo "   ✅ Metrics endpoint accessible"
    else
        echo "   ❌ Metrics endpoint not accessible"
        echo "   Check if Jaeger is configured with METRICS_BACKEND=prometheus"
    fi
else
    echo "   ❌ Could not find Jaeger pod"
fi

echo ""

# Check Prometheus targets (if accessible)
echo "5. Prometheus Integration:"
echo "   To verify Prometheus is scraping Jaeger:"
echo "   • Access Grafana UI: https://teachin.westeurope.cloudapp.azure.com/grafana"
echo "   • Go to Explore → Select Prometheus data source"
echo "   • Query: up{job=~\".*jaeger.*\"} to check if target is UP"
echo "   • Or create a dashboard with Jaeger metrics"
echo "   • ServiceMonitor target: 'serviceMonitor/observability/jaeger-metrics/0'"

echo ""
echo "🎯 ServiceMonitor test complete!"
echo ""
echo "📊 Available Jaeger metrics to monitor:"
echo "   • jaeger_collector_spans_received_total"
echo "   • jaeger_collector_spans_saved_by_svc"  
echo "   • jaeger_query_requests_total"
echo "   • jaeger_query_request_duration_seconds"
echo "   • process_cpu_seconds_total"
echo "   • process_memory_bytes"
