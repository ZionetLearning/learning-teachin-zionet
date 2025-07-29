#!/bin/bash

set -e

LOKI_NAMESPACE="devops-logs"
mkdir -p ./tmp-grafana

# === Step 1: Create values-loki.yaml ===
cat > ./tmp-grafana/values-loki.yaml <<EOF
grafana:
  enabled: true
  adminUser: admin
  adminPassword: admin123
  service:
    type: NodePort
    nodePort: 32000
  additionalDataSources:
    - name: Loki
      type: loki
      url: http://loki:3100
      access: proxy
      isDefault: true
  sidecar:
    dashboards:
      enabled: true
      searchNamespace: $LOKI_NAMESPACE

loki:
  enabled: true

promtail:
  enabled: true
  tolerations:
    - key: "node-role.kubernetes.io/control-plane"
      operator: "Exists"
      effect: "NoSchedule"
  extraVolumes:
    - name: varlog
      hostPath:
        path: /var/log
  extraVolumeMounts:
    - name: varlog
      mountPath: /var/log
      readOnly: true
  config:
    snippets:
      pipelineStages:
        - cri: {}
      extraScrapeConfigs: |
        - job_name: docker-containers
          static_configs:
            - targets:
                - localhost
              labels:
                job: varlogs
                __path__: /var/log/containers/*.log
          relabel_configs:
            - source_labels: [__path__]
              regex: '.*/(?P<container>[^_]+)_(?P<pod>[^_]+)_(?P<namespace>[^_]+)-.*\.log'
              target_label: container
              replacement: "$$1"
            - source_labels: [__path__]
              regex: '.*/(?P<container>[^_]+)_(?P<pod>[^_]+)_(?P<namespace>[^_]+)-.*\.log'
              target_label: pod
              replacement: "$$2"
            - source_labels: [__path__]
              regex: '.*/(?P<container>[^_]+)_(?P<pod>[^_]+)_(?P<namespace>[^_]+)-.*\.log'
              target_label: namespace
              replacement: "$$3"
EOF

# === Step 2: Create dashboard configmap ===
cat > ./tmp-grafana/pod-logs-dashboard.yaml <<EOF
apiVersion: v1
kind: ConfigMap
metadata:
  name: pod-logs-dashboard
  namespace: $LOKI_NAMESPACE
  labels:
    grafana_dashboard: "1"
data:
  pod-logs.json: |
    {
      "title": "Pod Logs",
      "uid": "podlogs",
      "schemaVersion": 16,
      "version": 1,
      "time": {
        "from": "now-1h",
        "to": "now"
      },
      "templating": {
        "list": [
          {
            "name": "pod",
            "label": "Pod",
            "type": "query",
            "datasource": "Loki",
            "refresh": 2,
            "query": "label_values(pod)",
            "definition": "label_values(pod)",
            "sort": 1,
            "hide": 0,
            "includeAll": true,
            "multi": false,
            "current": {
              "selected": true,
              "text": "All",
              "value": "$__all"
            }
          },
          {
            "name": "container",
            "label": "Container",
            "type": "query",
            "datasource": "Loki",
            "refresh": 2,
            "query": "label_values(container)",
            "definition": "label_values(container)",
            "sort": 1,
            "hide": 0,
            "includeAll": true,
            "multi": false,
            "current": {
              "selected": true,
              "text": "All",
              "value": "$__all"
            }
          }
        ]
      },
      "panels": [
        {
          "type": "logs",
          "title": "Logs by Pod and Container",
          "targets": [
            {
              "expr": "{pod=\"\$pod\", container=\"\$container\"}",
              "refId": "A"
            }
          ],
          "datasource": "Loki",
          "gridPos": { "x": 0, "y": 0, "w": 24, "h": 20 },
          "options": {
            "showLabels": true,
            "showTime": true,
            "wrapLogMessage": true,
            "dedupStrategy": "none"
          }
        }
      ]
    }
EOF


# === Step 3: Install Loki + Grafana ===
helm repo add grafana https://grafana.github.io/helm-charts
helm repo update

kubectl create namespace "$LOKI_NAMESPACE" || true
kubectl apply -f ./tmp-grafana/pod-logs-dashboard.yaml

helm upgrade --install loki-stack grafana/loki-stack \
  --namespace "$LOKI_NAMESPACE" \
  -f ./tmp-grafana/values-loki.yaml \
  --set grafana.sidecar.dashboards.enabled=true \
  --set grafana.sidecar.dashboards.searchNamespace="$LOKI_NAMESPACE" \
  --wait

echo ""
echo "✅ Logs setup complete!"
echo "🌍 Grafana: http://localhost:32000"
echo "👤 Login: admin / admin123"
echo "📊 Dashboard: Pod Logs → use dropdown to filter logs by pod"


## Check on PVC storage

## Check on building loki on existing 'Managed grafana' in azure on our aks