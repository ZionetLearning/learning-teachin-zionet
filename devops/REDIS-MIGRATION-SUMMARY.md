# Self-Hosted Redis Migration - Quick Reference

> **Full Documentation**: See [REDIS-MIGRATION-GUIDE.md](./REDIS-MIGRATION-GUIDE.md) for complete details.

## What Changed?

**Before**: Azure Cache for Redis (~$16-100/month per environment)  
**After**: Self-hosted Redis in Kubernetes (~$0/month)

## Files Modified

### Terraform (`terraform/`)

- ✏️ `main.tf` - Commented out Azure Redis, added self-hosted config
- ✏️ `variables.tf` - Added `selfhosted_redis_password` variable
- ✏️ `outputs.tf` - Updated Redis outputs
- ✏️ `secrets.tf` - Updated Key Vault secrets for self-hosted Redis

### Kubernetes (`kubernetes/charts/`)

- ➕ `templates/redis-deployment.yaml` - New Redis deployment
- ➕ `templates/redis-service.yaml` - New Redis service
- ✏️ `templates/dapr/statestore.yaml` - Disabled TLS
- ✏️ `values.yaml` - Updated Redis configuration

### No Workflow Changes Required

Workflows use Key Vault secrets which are automatically updated by Terraform.

## Quick Deploy

```bash
# 1. Set Redis password in GitHub Secrets or tfvars
TF_VAR_selfhosted_redis_password=<your-password>

# 2. Apply Terraform
cd devops/terraform
terraform apply

# 3. Deploy to Kubernetes
cd ../kubernetes/charts
helm upgrade <release> . --namespace <ns> --values values.yaml
```

## Quick Revert to Azure Redis

See the [Reverting section](./REDIS-MIGRATION-GUIDE.md#reverting-to-azure-cache-for-redis) in the full guide.

**Quick steps**:

1. Uncomment Azure Redis sections in Terraform files
2. Comment out self-hosted Redis configuration
3. Update Kubernetes values to enable TLS
4. Apply Terraform and Helm changes
5. Delete self-hosted Redis resources

## Key Configuration

| Setting          | Value               | Location              |
| ---------------- | ------------------- | --------------------- |
| **Service Name** | `redis-service`     | Kubernetes            |
| **Port**         | `6379`              | Kubernetes            |
| **TLS**          | Disabled            | Dapr statestore       |
| **Database**     | `0`                 | values.yaml           |
| **Memory**       | 256MB (512MB limit) | redis-deployment.yaml |
| **Storage**      | 2GB PVC             | redis-deployment.yaml |
| **Persistence**  | AOF (everysec)      | redis-deployment.yaml |

## Cost Savings

- **3 environments with Standard C1**: Save ~$2,952/year
- **Single dev environment**: Save ~$192-996/year

## Troubleshooting

```bash
# Check Redis pod
kubectl get pods -l app=redis -n <namespace>

# Test connection
kubectl exec -it <redis-pod> -- redis-cli -a <password> ping

# Check Dapr component
kubectl get component statestore -n <namespace>

# View logs
kubectl logs <redis-pod> -n <namespace>
```

## Important Notes

⚠️ **Not for production**: This setup is for learning/development environments only.  
✅ **Easy revert**: All Azure Redis code is preserved as comments.  
💾 **Data persistence**: Uses PVC with AOF for durability.  
🔒 **Security**: Password-protected, isolated within namespace.

---

For detailed information, troubleshooting, and revert instructions, see the complete [REDIS-MIGRATION-GUIDE.md](./REDIS-MIGRATION-GUIDE.md).
