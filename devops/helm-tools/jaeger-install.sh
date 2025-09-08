#!/bin/bash

# Bash script to install Jaeger in isolated namespace
# Run this to quickly set up Jaeger observability

echo "🚀 Installing Jaeger in isolated 'observability' namespace..."
echo "📍 URL will be: https://teachin.westeurope.cloudapp.azure.com/jaeger"
echo ""

# Apply the configuration
if kubectl apply -f ../kubernetes/config/jaeger-isolated-namespace.yaml; then
    echo ""

    # Create basic auth secret for Jaeger UI (if not exists)
    if ! kubectl get secret jaeger-basic-auth -n observability >/dev/null 2>&1; then
        echo "🔑 Creating Basic Auth secret for Jaeger UI..."
        USER="jaeger-admin"
        PASS="changeme123"
        htpasswd -bc auth $USER $PASS
        kubectl create secret generic jaeger-basic-auth \
            --from-file=auth \
            -n observability
        rm auth
        echo "   👉 Username: $USER | Password: $PASS"
    fi


    echo "⏳ Waiting for Jaeger to be ready..."
    
    if kubectl wait --for=condition=available deployment/jaeger-all-in-one -n observability --timeout=300s; then
        echo ""
        echo "✅ Jaeger installed successfully!"
        echo "🌐 Access at: https://teachin.westeurope.cloudapp.azure.com/jaeger"
        echo ""
        echo "📊 Check status:"
        echo "   kubectl get pods -n observability"
        echo "   kubectl get svc -n observability" 
        echo "   kubectl get ingress -n observability"
        echo ""
        echo "🗑️  To remove: ./jaeger-delete.sh"
    else
        echo "❌ Failed to wait for Jaeger deployment"
        exit 1
    fi
else
    echo "❌ Failed to apply Jaeger configuration"
    exit 1
fi

# Also apply Dapr tracing configuration
echo ""
echo "🔗 Enabling Dapr tracing..."
if kubectl apply -f ../kubernetes/config/dapr-tracing-config.yaml; then
    echo "✅ Dapr tracing enabled!"
else
    echo "⚠️  Dapr tracing configuration failed (Jaeger will still work)"
fi

echo ""
echo "🎯 Installation complete!"
