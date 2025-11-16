# Redis Migration Guide: Azure Cache to Self-Hosted

This guide documents the migration from Azure Cache for Redis to a self-hosted Redis instance running in the Kubernetes cluster. This change was made to reduce costs for this learning project.

## Table of Contents

- [Why Self-Host Redis?](#why-self-host-redis)
- [Important Considerations](#important-considerations)
- [Changes Made](#changes-made)
- [Deployment Steps](#deployment-steps)
- [Configuration Options](#configuration-options)
- [Reverting to Azure Cache for Redis](#reverting-to-azure-cache-for-redis)
- [Cost Comparison](#cost-comparison)
- [Troubleshooting](#troubleshooting)

---

## Why Self-Host Redis?

### Cost Savings

Azure Cache for Redis has significant costs even for the smallest tiers:

- **Basic C0 (250 MB)**: ~$15-20/month
- **Standard C1 (1 GB)**: ~$80-100/month
- **Additional environments** multiply these costs

For a learning/development project with low traffic and data requirements, these costs are unnecessary.

### Self-Hosted Benefits

- **Zero Azure costs** for Redis caching
- **Sufficient for learning**: Provides all Redis functionality needed for development
- **Easy revert**: Can switch back to Azure Redis anytime (see revert section)
- **Resource control**: Configure memory and CPU based on actual usage

### Trade-offs

- **No built-in HA**: Single instance (acceptable for non-production)
- **Manual management**: Updates and maintenance are your responsibility
- **Performance**: Lower performance than Azure Premium tiers (but sufficient for learning)
- **Backup**: Manual backup strategy needed (data persisted to PVC)

---

## Important Considerations

### When to Use Self-Hosted Redis

✅ **Good for:**

- Development environments
- Learning projects
- Low-traffic applications
- Cost-sensitive scenarios
- Non-production workloads

❌ **Not recommended for:**

- Production applications requiring high availability
- Applications with strict SLA requirements
- High-traffic scenarios requiring geo-replication
- Compliance requirements mandating managed services

### Data Persistence

- Self-hosted Redis uses **AOF (Append Only File)** persistence
- Data is stored on a **PersistentVolumeClaim (2GB)**
- Automatic fsync every second balances durability and performance
- **Note**: Pod restart may cause brief data loss (last 1 second)

### Security

- Password-protected (stored in Kubernetes secrets)
- No TLS by default (cluster-internal traffic)
- Isolated within Kubernetes namespace
- Not exposed externally

---

## Changes Made

### 1. Terraform Changes

#### `devops/terraform/main.tf`

```terraform
# Azure Redis module and data sources COMMENTED OUT
# Added self-hosted Redis configuration in locals
locals {
  redis_hostname = "redis-service.${local.kubernetes_namespace}.svc.cluster.local"
  redis_port     = 6379
  redis_key      = var.selfhosted_redis_password
}
```

**To revert**: Uncomment Azure Redis sections, comment out self-hosted configuration.

#### `devops/terraform/variables.tf`

```terraform
# Added new variable:
variable "selfhosted_redis_password" {
  description = "Password for self-hosted Redis instance"
  type        = string
  sensitive   = true
  default     = "change-me-in-production"
}

# Commented out:
# - use_shared_redis
# - shared_redis_name
```

#### `devops/terraform/outputs.tf`

```terraform
# Updated outputs to use self-hosted Redis values
# Azure Redis output lines commented with revert instructions
```

#### `devops/terraform/secrets.tf`

```terraform
# Updated Key Vault secrets:
# - redis_hostport: Now points to Kubernetes service (port 6379, not 6380)
# - redis_password: Uses selfhosted_redis_password variable
```

#### `devops/terraform/monitoring.tf`

```terraform
# redis_id set to null (no Azure resource to monitor)
# module.redis dependency removed
```

### 2. Kubernetes Changes

#### New Files Created

**`devops/kubernetes/charts/templates/redis-deployment.yaml`**

- Redis 7.2-alpine container
- Password authentication from secret
- 256MB memory limit (expandable)
- Liveness and readiness probes
- AOF persistence to PVC
- Resource requests: 100m CPU, 256Mi memory
- Resource limits: 500m CPU, 512Mi memory

**`devops/kubernetes/charts/templates/redis-service.yaml`**

- ClusterIP service exposing Redis on port 6379
- Service name: `redis-service`
- Internal cluster access only

#### Modified Files

**`devops/kubernetes/charts/templates/dapr/statestore.yaml`**

```yaml
# Changed enableTLS from 'true' to 'false'
- name: enableTLS
  value: "false" # Self-hosted Redis doesn't use TLS
```

**`devops/kubernetes/charts/values.yaml`**

```yaml
dapr:
  stateStore:
    redis:
      enableTLS: "false" # Changed from "true"
      redisDB: "0" # Changed from "5" - self-hosted uses DB 0
```

### 3. Workflow Changes

No changes required to workflows. The workflows use Key Vault secrets which are updated by Terraform to point to the self-hosted Redis instance.

**Note**: Database index environment variable in `aks-helmcharts.yaml` is still set per environment but all environments now use the same self-hosted Redis instance (just different DB indices if needed).

---

## Deployment Steps

### Step 1: Update Terraform Variables

Add the Redis password to your Terraform variables or GitHub Secrets:

```bash
# For GitHub Actions, add secret:
TF_VAR_selfhosted_redis_password=<your-secure-password>

# Or in terraform.tfvars:
selfhosted_redis_password = "your-secure-redis-password"
```

**Security Note**: Use a strong password. This will be stored in Azure Key Vault and referenced by Kubernetes.

### Step 2: Apply Terraform Changes

```bash
cd devops/terraform

# Initialize (if needed)
terraform init

# Review changes
terraform plan

# Apply changes
terraform apply
```

**What happens:**

- Azure Redis resources are no longer created/managed
- Key Vault secrets updated to point to self-hosted Redis
- Outputs reflect new Redis hostname and port

### Step 3: Deploy Redis to Kubernetes

```bash
cd devops/kubernetes/charts

# Upgrade Helm release with new Redis components
helm upgrade <release-name> . \
  --namespace <your-namespace> \
  --values values.yaml \
  --values values.<environment>.yaml
```

**What gets deployed:**

- Redis Deployment (1 replica)
- Redis Service (ClusterIP)
- PersistentVolumeClaim (2GB)
- Updated Dapr statestore component

### Step 4: Verify Deployment

```bash
# Check Redis pod status
kubectl get pods -n <namespace> -l app=redis

# Check Redis service
kubectl get svc -n <namespace> redis-service

# Check PVC
kubectl get pvc -n <namespace> redis-pvc

# Test Redis connection
kubectl exec -n <namespace> -it <redis-pod-name> -- redis-cli -a <password> ping
# Should return: PONG
```

### Step 5: Verify Application Connectivity

```bash
# Check Dapr statestore component
kubectl get component -n <namespace> statestore

# Check application pods can connect to Redis
kubectl logs -n <namespace> <accessor-pod-name> | grep -i redis
kubectl logs -n <namespace> <manager-pod-name> | grep -i redis
```

### Step 6: Remove Azure Redis Resources (Optional)

If you had existing Azure Redis resources, you can now delete them manually:

```bash
# List existing Redis caches
az redis list --output table

# Delete specific instance
az redis delete \
  --name <redis-name> \
  --resource-group <resource-group-name>
```

---

## Configuration Options

### Memory and CPU Resources

Edit `redis-deployment.yaml`:

```yaml
resources:
  requests:
    cpu: 100m # Adjust based on load
    memory: 256Mi # Adjust based on data size
  limits:
    cpu: 500m # Maximum CPU
    memory: 512Mi # Maximum memory
```

### Redis Memory Policy

Current configuration: `allkeys-lru` (evicts least recently used keys when memory limit reached)

Other options:

- `allkeys-lfu`: Least Frequently Used
- `volatile-lru`: LRU among keys with expire set
- `noeviction`: Return errors when memory limit reached

Edit in `redis-deployment.yaml`:

```yaml
command:
  - redis-server
  - "--maxmemory-policy"
  - "allkeys-lru" # Change here
```

### Persistence Settings

Current: AOF with `everysec` fsync

Edit for different persistence:

```yaml
# Disable persistence (faster, but no durability)
- "--appendonly"
- "no"

# More aggressive persistence (slower, more durable)
- "--appendfsync"
- "always" # Fsync on every write
```

### Storage Size

Edit PVC in `redis-deployment.yaml`:

```yaml
resources:
  requests:
    storage: 2Gi # Adjust size here
```

### Password Rotation

1. Update the password in Terraform variable
2. Run `terraform apply` to update Key Vault
3. Update Kubernetes secret:
   ```bash
   kubectl create secret generic redis-connection \
     --from-literal=redis-password=<new-password> \
     --namespace=<namespace> \
     --dry-run=client -o yaml | kubectl apply -f -
   ```
4. Restart Redis pod:
   ```bash
   kubectl rollout restart deployment/redis -n <namespace>
   ```

---

## Reverting to Azure Cache for Redis

If you need to switch back to Azure Cache for Redis (e.g., for production deployment):

### Step 1: Update Terraform Files

#### `devops/terraform/main.tf`

**Uncomment** the Azure Redis section:

```terraform
# ------------- Shared Redis -----------------------
data "azurerm_redis_cache" "shared" {
  count               = var.use_shared_redis ? 1 : 0
  name                = var.redis_name
  resource_group_name = var.shared_resource_group
}

module "redis" {
  count                = var.use_shared_redis ? 0 : 1
  source               = "./modules/redis"
  name                 = var.redis_name
  location             = azurerm_resource_group.main.location
  resource_group_name  = azurerm_resource_group.main.name
  shared_redis_name    = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].name : null
}

locals {
  redis_hostname = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].hostname : module.redis[0].hostname
  redis_port     = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].port : module.redis[0].port
  redis_key      = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : module.redis[0].primary_access_key
}
```

**Comment out** the self-hosted Redis configuration.

#### `devops/terraform/variables.tf`

**Uncomment**:

```terraform
variable "use_shared_redis" {
  description = "Use shared Redis instance instead of creating new one"
  type        = bool
  default     = true
}

variable "shared_redis_name" {
  type        = string
  default     = null
  description = "Name of shared Redis cache, if using shared"
}
```

**Comment out** or remove:

```terraform
# variable "selfhosted_redis_password" { ... }
```

#### `devops/terraform/outputs.tf`

Revert to Azure Redis outputs:

```terraform
output "redis_hostname" {
  value = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].hostname : module.redis[0].hostname
}

output "redis_primary_access_key" {
  value     = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : module.redis[0].primary_access_key
  sensitive = true
}
```

#### `devops/terraform/secrets.tf`

```terraform
resource "azurerm_key_vault_secret" "redis_hostport" {
  name         = "${var.environment_name}-redis-hostport"
  value        = var.use_shared_redis ? "${data.azurerm_redis_cache.shared[0].hostname}:6380" : "${module.redis[0].hostname}:6380"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "redis_password" {
  name         = "${var.environment_name}-redis-password"
  value        = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : module.redis[0].primary_access_key
  key_vault_id = data.azurerm_key_vault.shared.id
}
```

#### `devops/terraform/main.tf` (monitoring section)

```terraform
redis_id = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].id : module.redis[0].id
```

Add back `module.redis` to `depends_on`.

### Step 2: Update Kubernetes Configuration

#### `devops/kubernetes/charts/values.yaml`

```yaml
dapr:
  stateStore:
    redis:
      enableTLS: "true" # Enable TLS for Azure Redis
      redisDB: "5" # Or your preferred DB index
```

#### `devops/kubernetes/charts/templates/dapr/statestore.yaml`

```yaml
- name: enableTLS
  value: '{{ default "true" .Values.dapr.stateStore.redis.enableTLS }}'
```

### Step 3: Remove Self-Hosted Redis from Kubernetes

You can either:

**Option A: Delete the files** (cleanest)

```bash
rm devops/kubernetes/charts/templates/redis-deployment.yaml
rm devops/kubernetes/charts/templates/redis-service.yaml
```

**Option B: Keep files but don't deploy them** (add condition)

Add to the top of both files:

```yaml
{{- if .Values.selfhostedRedis.enabled }}
# ... existing content ...
{{- end }}
```

And in `values.yaml`:

```yaml
selfhostedRedis:
  enabled: false # Set to true to use self-hosted
```

### Step 4: Apply Changes

```bash
# 1. Apply Terraform
cd devops/terraform
terraform apply

# 2. Update Helm deployment
cd ../kubernetes/charts
helm upgrade <release-name> . \
  --namespace <namespace> \
  --values values.yaml

# 3. Delete self-hosted Redis (if keeping files)
kubectl delete deployment redis -n <namespace>
kubectl delete service redis-service -n <namespace>
kubectl delete pvc redis-pvc -n <namespace>
```

### Step 5: Verify Azure Redis Connectivity

```bash
# Check application logs
kubectl logs -n <namespace> <accessor-pod-name> | grep -i redis

# Verify Dapr component
kubectl describe component statestore -n <namespace>
```

---

## Cost Comparison

### Azure Cache for Redis Costs (approximate, US region)

| Tier            | Size   | Memory | Monthly Cost\* | Annual Cost\* |
| --------------- | ------ | ------ | -------------- | ------------- |
| **Basic C0**    | 250 MB | 250 MB | $16            | $192          |
| **Basic C1**    | 1 GB   | 1 GB   | $41            | $492          |
| **Standard C1** | 1 GB   | 1 GB   | $83            | $996          |
| **Standard C2** | 2.5 GB | 2.5 GB | $165           | $1,980        |
| **Premium P1**  | 6 GB   | 6 GB   | $368           | $4,416        |

\*Prices are approximate and vary by region. Check Azure pricing calculator for exact costs.

### Self-Hosted Redis Costs

**Direct costs**: $0 (uses existing AKS cluster resources)

**Indirect costs** (shared AKS cluster resources):

- **Storage**: ~$0.10/GB/month for persistent volume (2GB = $0.20/month)
- **Compute**: Minimal (100m CPU, 256Mi memory from existing node pool)

**Total estimated cost**: < $1/month

### Cost Savings Example

For a project with 3 environments (dev, staging, prod) using Standard C1:

- **Azure Redis**: 3 × $83 = $249/month ($2,988/year)
- **Self-hosted**: < $3/month ($36/year)
- **Savings**: ~$246/month (~$2,952/year)

### Performance Comparison

| Metric              | Azure Redis (C1) | Self-Hosted     | Notes                |
| ------------------- | ---------------- | --------------- | -------------------- |
| **Memory**          | 1 GB             | 256-512 MB      | Configurable         |
| **Max Connections** | 1,000            | ~500            | Based on resources   |
| **Bandwidth**       | 100 Mbps         | Cluster network | Usually sufficient   |
| **Latency**         | < 1ms            | < 1ms           | Both in same cluster |
| **SLA**             | 99.9%            | None            | Self-managed         |
| **Backup**          | Automated        | Manual          | Need to implement    |
| **Scaling**         | Vertical         | Horizontal      | Different approaches |

---

## Troubleshooting

### Redis Pod Not Starting

**Check pod status:**

```bash
kubectl get pods -n <namespace> -l app=redis
kubectl describe pod <redis-pod-name> -n <namespace>
```

**Common issues:**

- **PVC not binding**: Check storage class availability
  ```bash
  kubectl get pvc -n <namespace>
  kubectl get storageclass
  ```
- **Image pull error**: Check internet connectivity or use a cached image
- **Memory limits**: Increase if OOMKilled

### Authentication Failures

**Check secret exists:**

```bash
kubectl get secret redis-connection -n <namespace>
kubectl get secret redis-connection -n <namespace> -o yaml
```

**Verify password:**

```bash
# Decode and check password
kubectl get secret redis-connection -n <namespace> -o jsonpath='{.data.redis-password}' | base64 --decode
```

**Test connection manually:**

```bash
kubectl exec -n <namespace> -it <redis-pod-name> -- redis-cli -a <password> ping
```

### Application Cannot Connect

**Check Dapr component:**

```bash
kubectl get component statestore -n <namespace> -o yaml
kubectl describe component statestore -n <namespace>
```

**Check application logs:**

```bash
kubectl logs -n <namespace> <app-pod-name> -c daprd | grep -i redis
```

**Verify service:**

```bash
kubectl get svc redis-service -n <namespace>
kubectl get endpoints redis-service -n <namespace>
```

**Test connection from app pod:**

```bash
kubectl exec -n <namespace> -it <app-pod-name> -- \
  nc -zv redis-service 6379
```

### Data Loss

**Check persistence:**

```bash
# Check if AOF is enabled
kubectl exec -n <namespace> -it <redis-pod-name> -- \
  redis-cli -a <password> CONFIG GET appendonly

# Should return: appendonly yes
```

**Check PVC:**

```bash
kubectl get pvc redis-pvc -n <namespace>
# Status should be "Bound"
```

**Backup data manually:**

```bash
# Create backup
kubectl exec -n <namespace> <redis-pod-name> -- \
  redis-cli -a <password> --rdb /data/backup.rdb

# Copy backup locally
kubectl cp <namespace>/<redis-pod-name>:/data/backup.rdb ./redis-backup.rdb
```

### Performance Issues

**Check resource usage:**

```bash
kubectl top pod -n <namespace> -l app=redis
```

**Check Redis stats:**

```bash
kubectl exec -n <namespace> -it <redis-pod-name> -- \
  redis-cli -a <password> INFO stats
```

**Increase resources if needed** (edit `redis-deployment.yaml`):

```yaml
resources:
  limits:
    cpu: 1000m # Increase CPU
    memory: 1024Mi # Increase memory
```

### High Memory Usage

**Check memory stats:**

```bash
kubectl exec -n <namespace> -it <redis-pod-name> -- \
  redis-cli -a <password> INFO memory
```

**Check maxmemory policy:**

```bash
kubectl exec -n <namespace> -it <redis-pod-name> -- \
  redis-cli -a <password> CONFIG GET maxmemory-policy
```

**Flush data if needed (WARNING: deletes all data):**

```bash
kubectl exec -n <namespace> -it <redis-pod-name> -- \
  redis-cli -a <password> FLUSHALL
```

---

## Additional Resources

### Redis Documentation

- [Redis Official Documentation](https://redis.io/documentation)
- [Redis Persistence](https://redis.io/topics/persistence)
- [Redis Security](https://redis.io/topics/security)

### Kubernetes Resources

- [Kubernetes Persistent Volumes](https://kubernetes.io/docs/concepts/storage/persistent-volumes/)
- [Kubernetes StatefulSets](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/) (for HA Redis)

### Dapr Documentation

- [Dapr Redis State Store](https://docs.dapr.io/reference/components-reference/supported-state-stores/setup-redis/)
- [Dapr Component Secrets](https://docs.dapr.io/operations/components/component-secrets/)

### Migration Scripts

Consider creating a data migration script if moving data from Azure Redis to self-hosted:

```bash
# Export from Azure Redis
redis-cli -h <azure-redis-host> -p 6380 -a <azure-key> --tls --rdb azure-dump.rdb

# Import to self-hosted
kubectl cp azure-dump.rdb <namespace>/<redis-pod-name>:/data/import.rdb
kubectl exec -n <namespace> -it <redis-pod-name> -- \
  redis-cli -a <password> --rdb /data/import.rdb
```

---

## Support and Feedback

For questions or issues with this migration:

1. Check the [Troubleshooting](#troubleshooting) section
2. Review Redis and application logs
3. Consult with the team lead or DevOps engineer

---

**Last Updated**: 2025-01-16  
**Version**: 1.0  
**Status**: Production-ready for learning environments
