# Self-Hosted Redis Deployment Guide

## Architecture Overview

We use a **single shared Redis instance** deployed in a dedicated `redis` namespace. All application environments (dev, prod, test, featest, featest2) connect to this shared instance using different database indexes (0-5) for data isolation.

### Benefits

- **Cost Optimization**: One Redis instance instead of 5+ (saves ~$17/month Azure Redis + no per-environment overhead)
- **Resource Efficiency**: Utilizes spare AKS capacity instead of deploying per-namespace
- **Simplified Management**: Single Redis to monitor, backup, and upgrade
- **Data Isolation**: Each environment uses a separate database index (0-5)

### Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│               AKS Cluster                           │
│                                                     │
│  ┌──────────────────────────────────────────────┐  │
│  │  redis namespace                             │  │
│  │  ┌─────────────────────────────────────────┐ │  │
│  │  │ Redis StatefulSet (redis-0)             │ │  │
│  │  │ - Service: redis.redis.svc.cluster.local│ │  │
│  │  │ - Port: 6379                             │ │  │
│  │  │ - Storage: 8Gi PVC                       │ │  │
│  │  └─────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────┘  │
│                       ▲                             │
│                       │ connects to                 │
│  ┌────────────────────┼──────────────────────────┐ │
│  │ dev namespace      │                          │ │
│  │ - Manager (DB: 0) ─┘                          │ │
│  │ - Engine                                      │ │
│  │ - Accessor                                    │ │
│  └───────────────────────────────────────────────┘ │
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │ prod namespace                               │   │
│  │ - Manager (DB: 1)                            │   │
│  │ - Engine                                     │   │
│  │ - Accessor                                   │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  ... (test, featest, featest2 follow same pattern) │
└─────────────────────────────────────────────────────┘
```

## Database Index Mapping

Each environment uses a dedicated Redis database index for data isolation:

| Environment | Database Index |
| ----------- | -------------- |
| dev         | 0              |
| prod        | 1              |
| test        | 2              |
| featest     | 3              |
| featest2    | 4              |
| other       | 5              |

## Deployment Steps

### 1. Create Redis Namespace (One-Time Setup)

```bash
kubectl create namespace redis
```

### 2. Deploy Redis Instance (One-Time Setup)

Deploy the shared Redis instance to the `redis` namespace:

```bash
# Generate a secure Redis password
REDIS_PASSWORD=$(openssl rand -base64 32)

# Deploy Redis to the redis namespace
helm upgrade --install redis-shared ./devops/kubernetes/charts \
  --namespace redis \
  --set redis.enabled=true \
  --set redis.useAzureRedis=false \
  --set redis.password="$REDIS_PASSWORD" \
  --set dapr.installComponents=false \
  --set manager.enabled=false \
  --set engine.enabled=false \
  --set accessor.enabled=false \
  --set apigateway.enabled=false

# Save the password for use in application deployments
echo "$REDIS_PASSWORD" > /tmp/redis-password.txt
echo "Redis password saved to /tmp/redis-password.txt"
```

### 3. Verify Redis Deployment

```bash
# Check Redis pod status
kubectl get pods -n redis

# Expected output:
# NAME      READY   STATUS    RESTARTS   AGE
# redis-0   1/1     Running   0          1m

# Check Redis service
kubectl get svc -n redis

# Test Redis connectivity
kubectl exec -n redis redis-0 -- redis-cli -a "$REDIS_PASSWORD" ping
# Expected: PONG
```

### 4. Deploy Applications to Each Environment

Each environment deployment will:

- Create the redis-secret in its namespace (with the shared password)
- Configure Dapr to connect to `redis.redis.svc.cluster.local:6379`
- Use the environment-specific database index

```bash
# Example for dev environment
REDIS_PASSWORD=$(cat /tmp/redis-password.txt)

helm upgrade --install app ./devops/kubernetes/charts \
  --namespace dev \
  -f ./devops/kubernetes/charts/values.dev.yaml \
  --set redis.password="$REDIS_PASSWORD" \
  --set dapr.stateStore.redis.redisDB=0 \
  --set global.environment="dev"

# Example for prod environment
helm upgrade --install app ./devops/kubernetes/charts \
  --namespace prod \
  -f ./devops/kubernetes/charts/values.prod.yaml \
  --set redis.password="$REDIS_PASSWORD" \
  --set dapr.stateStore.redis.redisDB=1 \
  --set global.environment="prod"
```

**Note**: The GitHub Actions workflow already handles this automatically with the correct database indexes.

### 5. Verify Application Connectivity

```bash
# Check Dapr sidecar logs for Redis connection
kubectl logs -n dev <pod-name> -c daprd | grep -i redis

# Check application can access Redis via Dapr
kubectl logs -n dev <manager-pod-name> -c manager | grep -i state
```

## How It Works

### Helm Chart Logic

The Helm chart uses conditional logic to deploy Redis only in the `redis` namespace:

```yaml
# Redis resources only deploy when:
# 1. redis.enabled=true
# 2. redis.useAzureRedis=false
# 3. .Release.Namespace == redis.namespace (i.e., "redis")
{
  {
    - if and .Values.redis.enabled (not .Values.redis.useAzureRedis) (eq .Release.Namespace .Values.redis.namespace),
  },
}
```

### Dapr State Store Configuration

Applications in all namespaces connect to the shared Redis:

```yaml
# Dapr statestore component
spec:
  type: state.redis
  metadata:
    - name: redisHost
      value: "redis.redis.svc.cluster.local:6379" # Shared Redis DNS
    - name: redisPassword
      secretKeyRef:
        name: redis-secret # Secret exists in each namespace
        key: redis-password
    - name: redisDB
      value: "0" # Environment-specific (0-5)
```

### Secret Management

- The `redis-secret` is created in **every namespace** (redis, dev, prod, test, etc.)
- All secrets contain the same password (managed by CI/CD)
- The Redis instance validates connections using this password

## Maintenance

### Backup Redis Data

```bash
# Create a backup
kubectl exec -n redis redis-0 -- redis-cli -a "$REDIS_PASSWORD" SAVE

# Copy backup from pod
kubectl cp redis/redis-0:/data/dump.rdb ./redis-backup-$(date +%Y%m%d).rdb
```

### Monitor Redis Performance

```bash
# Check Redis stats
kubectl exec -n redis redis-0 -- redis-cli -a "$REDIS_PASSWORD" INFO stats

# Monitor memory usage
kubectl exec -n redis redis-0 -- redis-cli -a "$REDIS_PASSWORD" INFO memory

# Check connected clients
kubectl exec -n redis redis-0 -- redis-cli -a "$REDIS_PASSWORD" CLIENT LIST
```

### Scale Redis (if needed)

```bash
# Update resources in values.yaml
redis:
  resources:
    requests:
      cpu: "200m"
      memory: "512Mi"
    limits:
      cpu: "1000m"
      memory: "1Gi"

# Redeploy
helm upgrade redis-shared ./devops/kubernetes/charts --namespace redis -f values.yaml
```

### Troubleshooting

#### Redis pod not starting

```bash
kubectl describe pod -n redis redis-0
kubectl logs -n redis redis-0
```

#### Application can't connect to Redis

```bash
# Verify DNS resolution from application pod
kubectl exec -n dev <pod-name> -- nslookup redis.redis.svc.cluster.local

# Check if secret exists
kubectl get secret -n dev redis-secret

# Verify Dapr component
kubectl get component -n dev statestore -o yaml
```

#### Data isolation verification

```bash
# Check which databases have keys
for i in {0..5}; do
  echo "Database $i:"
  kubectl exec -n redis redis-0 -- redis-cli -a "$REDIS_PASSWORD" -n $i DBSIZE
done
```

## Rollback to Azure Redis

If you need to switch back to Azure Redis Cache:

1. Update Terraform to uncomment Azure Redis resources in `main.tf` and `secrets.tf`
2. Run `terraform apply`
3. Update Helm values: `redis.useAzureRedis: true`
4. Redeploy applications with GitHub Actions

The code is preserved as comments in Terraform files for easy rollback.

## Cost Comparison

| Solution                   | Monthly Cost     | Notes               |
| -------------------------- | ---------------- | ------------------- |
| Azure Redis Cache (shared) | $17.33           | Basic tier, 250MB   |
| Self-hosted on AKS         | ~$0              | Uses spare capacity |
| **Savings**                | **$17.33/month** | **100% reduction**  |

## Next Steps

- [ ] Monitor Redis performance in production
- [ ] Set up Redis backup automation
- [ ] Configure Redis persistence settings if needed
- [ ] Consider Redis Sentinel for HA (future enhancement)
