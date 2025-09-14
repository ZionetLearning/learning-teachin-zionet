# CI/CD Integration Guide for PostgreSQL Private Networking

## Overview
This guide addresses critical issues when deploying with PostgreSQL private networking in CI/CD pipelines.

## Issues & Solutions

### 1. PostgreSQL Private FQDN Issue

**Problem**: Azure auto-generates DNS records for VNet-integrated PostgreSQL servers, causing connection failures.

**Root Cause**: Terraform outputs the constructed FQDN (`server-name.privatelink.postgres.database.azure.com`) but Azure creates auto-generated records (`random-id.privatelink.postgres.database.azure.com`).

**Solution**: Run the FQDN fix script after Terraform deployment.

### 2. Container Image Tag Management

**Problem**: Helm values reference non-existent image tags, causing `ImagePullBackOff` errors.

**Root Cause**: Disconnect between CI/CD build tags and Helm configuration.

**Solution**: Use consistent tagging strategy and update Helm values accordingly.

## CI/CD Pipeline Integration

### Complete Pipeline Flow

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      # 1. Build and Push Images
      - name: Build and Push Docker Images
        run: |
          docker build -t kinwon/accessor:${{ github.sha }} ./backend/ContainerApp/Accessor
          docker build -t kinwon/engine:${{ github.sha }} ./backend/ContainerApp/Engine  
          docker build -t kinwon/manager:${{ github.sha }} ./backend/ContainerApp/Manager
          
          docker push kinwon/accessor:${{ github.sha }}
          docker push kinwon/engine:${{ github.sha }}
          docker push kinwon/manager:${{ github.sha }}
          
          # Also tag as 'prod' for consistency
          docker tag kinwon/accessor:${{ github.sha }} kinwon/accessor:prod
          docker tag kinwon/engine:${{ github.sha }} kinwon/engine:prod
          docker tag kinwon/manager:${{ github.sha }} kinwon/manager:prod
          
          docker push kinwon/accessor:prod
          docker push kinwon/engine:prod
          docker push kinwon/manager:prod

      # 2. Deploy Infrastructure
      - name: Terraform Apply
        run: |
          cd devops/terraform
          terraform init
          terraform apply -auto-approve -var-file=terraform.tfvars.prod

      # 3. Fix PostgreSQL Private FQDN
      - name: Fix PostgreSQL Private FQDN
        env:
          ENVIRONMENT_NAME: ${{ env.environment_name }}
        run: |
          chmod +x ./devops/fix-postgres-private-fqdn.sh
          ./devops/fix-postgres-private-fqdn.sh \
            "$ENVIRONMENT_NAME" \
            "${ENVIRONMENT_NAME}-zionet-learning-2025" \
            "privatelink.postgres.database.azure.com" \
            "teachin-seo-kv" \
            "${ENVIRONMENT_NAME}-postgres-connection"

      # 4. Deploy Applications
      - name: Deploy Applications with Helm
        run: |
          helm upgrade --install app ./devops/kubernetes/charts \
            -f ./devops/kubernetes/charts/values.prod.yaml \
            -n prod \
            --set manager.image.tag=${{ github.sha }} \
            --set engine.image.tag=${{ github.sha }} \
            --set accessor.image.tag=${{ github.sha }}

      # 5. Verify Deployment
      - name: Verify Deployment
        run: |
          kubectl rollout status deployment/manager -n prod --timeout=300s
          kubectl rollout status deployment/engine -n prod --timeout=300s
          kubectl rollout status deployment/accessor -n prod --timeout=300s
```

### Alternative: Environment-based Tags

For simpler management, use environment-based tags:

```yaml
      # Build with environment tags
      - name: Build and Push Docker Images
        run: |
          docker build -t kinwon/accessor:prod ./backend/ContainerApp/Accessor
          docker build -t kinwon/engine:prod ./backend/ContainerApp/Engine  
          docker build -t kinwon/manager:prod ./backend/ContainerApp/Manager
          
          docker push kinwon/accessor:prod
          docker push kinwon/engine:prod
          docker push kinwon/manager:prod

      # Deploy with fixed tags in Helm values
      - name: Deploy Applications
        run: |
          helm upgrade --install app ./devops/kubernetes/charts \
            -f ./devops/kubernetes/charts/values.prod.yaml \
            -n prod
```

## Required Scripts

### 1. PostgreSQL FQDN Fix Script
Location: `devops/fix-postgres-private-fqdn.sh`

This script automatically:
- Discovers auto-generated PostgreSQL FQDN
- Updates Key Vault connection string
- Refreshes Kubernetes secrets

### 2. Environment Configuration
Ensure these environment variables are set in CI/CD:
```bash
ARM_CLIENT_ID=<service-principal-id>
ARM_CLIENT_SECRET=<service-principal-secret>
ARM_SUBSCRIPTION_ID=<azure-subscription-id>
ARM_TENANT_ID=<azure-tenant-id>
```

## Verification Steps

After deployment, verify:

1. **PostgreSQL Connectivity**:
   ```bash
   kubectl logs deployment/accessor -n prod
   # Should show successful database migrations
   ```

2. **Image Pull Success**:
   ```bash
   kubectl get pods -n prod
   # All pods should be Running (2/2 Ready)
   ```

3. **Private FQDN Correctness**:
   ```bash
   az keyvault secret show --vault-name teachin-seo-kv --name prod-postgres-connection
   # Host should be auto-generated FQDN (e.g., e96178425ffb.privatelink.postgres.database.azure.com)
   ```

## Troubleshooting

### ImagePullBackOff Errors
- Verify image tags exist in registry
- Check Helm values match pushed tags
- Ensure registry authentication is configured

### PostgreSQL Connection Failures  
- Run the FQDN fix script manually
- Verify private DNS zone has auto-generated records
- Check VNet peering and DNS links

### External Secrets Not Syncing
- Delete and recreate Kubernetes secrets
- Check External Secrets Operator logs
- Verify Key Vault permissions

## Best Practices

1. **Use consistent tagging strategy** (commit SHA or environment-based)
2. **Always run PostgreSQL FQDN fix** after infrastructure changes
3. **Verify deployment status** before considering pipeline successful
4. **Monitor application logs** for connectivity issues
5. **Test in staging environment** with same private networking setup
