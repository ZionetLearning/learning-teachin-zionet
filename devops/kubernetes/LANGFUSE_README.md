# Langfuse Integration

This integration adds Langfuse to your Kubernetes cluster for the dev environment, accessible at:
https://teachin.westeurope.cloudapp.azure.com/langfuse

## Prerequisites

1. **Azure Key Vault**: Make sure your Key Vault has the required secrets.
2. **PostgreSQL Database**: Langfuse will use the same PostgreSQL server and database (`appdb-dev`) as your other services.
3. **External Secrets Operator**: Should already be configured in your cluster.
4. **Ingress Controller**: NGINX ingress controller should be running.
5. **Cert-Manager**: For TLS certificates.

## What's Included

### Kubernetes Templates
- `langfuse-deployment.yaml`: Main Langfuse deployment and service
- `langfuse-ingress.yaml`: Ingress configuration for external access
- `langfuse-db-migrate-job.yaml`: Database migration job (runs before deployment)
- `externalsecret-langfuse.yaml`: External secrets for database and auth configuration

### Configuration Files
- Updated `values.yaml`: Base Langfuse configuration
- Updated `values.dev.yaml`: Dev-specific Langfuse settings
- Updated `secrets.tf`: Terraform configuration for Langfuse secrets

## Deployment Steps

### GitHub Actions (Recommended)

Langfuse is deployed as platform tooling (like Grafana) during the platform setup phase:

**Option 1: Platform Setup Only**
1. Go to GitHub Actions → "AKS - Platform Setup"
2. Choose "Development" environment
3. Set environment_name to "dev" 
4. Click "Run workflow"
5. Langfuse will be deployed automatically (dev environment only)

**Option 2: Full CI/CD Pipeline**
1. Go to GitHub Actions → "Full CICD"
2. Choose "Development" environment  
3. Set environment_name to "dev"
4. Click "Run workflow"
5. Langfuse will be deployed during the platform setup phase

### Manual Deployment

**Prerequisites**: Langfuse secrets must be created first via Terraform:
- `terraform.tfvars.dev`: `enable_langfuse = true` 
- `terraform.tfvars.prod`: `enable_langfuse = false`

#### 1. Apply Terraform Changes (Secrets)

```bash
cd devops/terraform
terraform plan -var-file="terraform.tfvars.dev"
terraform apply -var-file="terraform.tfvars.dev"
```

This creates (only when `enable_langfuse = true` and `environment_name = "dev"`):
- `langfuse-nextauth-secret`: Random secret for NextAuth.js authentication
- `langfuse-salt`: Random salt for password hashing
- `dev-postgres-username`: PostgreSQL username for Langfuse
- `dev-postgres-password`: PostgreSQL password for Langfuse

#### 2. Deploy Langfuse Platform Components

```bash
# Apply secrets
kubectl apply -f devops/kubernetes/manifests/langfuse-secrets.yaml

# Deploy Langfuse via Helm
cd devops/helm-tools
chmod +x langfuse.sh
./langfuse.sh

# Apply ingress
kubectl apply -f devops/kubernetes/ingress/langfuse-ingress.yaml
```

### 3. Verify Deployment

Check that all components are running:

```bash
# Check pods
kubectl get pods -n dev | grep langfuse

# Check services
kubectl get svc -n dev | grep langfuse

# Check ingress
kubectl get ingress -n dev | grep langfuse

# Check external secrets
kubectl get externalsecrets -n dev | grep langfuse
```

### 4. Access Langfuse

Once deployed, Langfuse will be available at:
https://teachin.westeurope.cloudapp.azure.com/langfuse

## Architecture

### Deployment Model
- **Platform Tooling**: Deployed like Grafana in `devops-tools` namespace
- **Independent**: Separate from your main application Helm chart
- **Infrastructure Phase**: Deployed during platform setup (not application deployment)

### Database Connection
- **Database**: Uses existing PostgreSQL server `dev-pg-zionet-learning.postgres.database.azure.com`
- **Database Name**: `appdb-dev` (same as your other services)
- **Credentials**: Retrieved from Azure Key Vault via External Secrets Operator

### Authentication
- **NextAuth Secret**: Auto-generated random secret (32 chars) 
- **Salt**: Auto-generated random salt (32 chars)
- **URL**: `https://teachin.westeurope.cloudapp.azure.com/langfuse`

### Resources
- **CPU**: 100m request, 500m limit
- **Memory**: 256Mi request, 512Mi limit  
- **Namespace**: `devops-tools` (same as Grafana)
- **Scaling**: Can be scaled via Helm values

## Customization

### Environment Variables
You can add additional environment variables in `values.dev.yaml`:

```yaml
langfuse:
  env:
    additionalEnvVars:
      - name: CUSTOM_VAR
        value: "custom_value"
```

### Resource Limits
Adjust resources in the values files:

```yaml
langfuse:
  resources:
    requests:
      cpu: "200m"
      memory: "512Mi"
    limits:
      cpu: "1000m"
      memory: "1Gi"
```

### Scaling
To scale the deployment:

```yaml
langfuse:
  replicas: 2
```

## Troubleshooting

### Check Logs
```bash
kubectl logs -n dev deployment/langfuse
```

### Check Database Migration Job
```bash
kubectl get jobs -n dev | grep langfuse
kubectl logs -n dev job/langfuse-db-migrate
```

### Check External Secrets
```bash
kubectl describe externalsecrets -n dev langfuse-database-secret
kubectl describe externalsecrets -n dev langfuse-auth-secret
```

### Check Ingress
```bash
kubectl describe ingress -n dev langfuse-ingress
```

## Notes

- Langfuse is only enabled for the dev environment
- The database migration job runs automatically before each deployment
- TLS is configured using the same certificate as your other services
- The configuration follows your existing patterns for external secrets and ingress
