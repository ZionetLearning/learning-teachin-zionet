# PostgreSQL Private Networking CI/CD Integration

## Problem
When PostgreSQL Flexible Server is configured with private networking (VNet integration), Azure auto-generates DNS records instead of using the server name. This causes Terraform outputs to provide incorrect FQDNs.

## Example
- **Expected**: `prod-pg-zionet-learning.privatelink.postgres.database.azure.com`
- **Actual**: `e96178425ffb.privatelink.postgres.database.azure.com`

## Solution
The `fix-postgres-private-fqdn.sh` script handles this automatically by:
1. Querying the private DNS zone for auto-generated records
2. Extracting the correct FQDN
3. Updating the Key Vault secret with the correct connection string
4. Refreshing Kubernetes secrets via External Secrets

## CI/CD Integration

### GitHub Actions
Add this step after Terraform apply:

```yaml
- name: Fix PostgreSQL Private FQDN
  run: |
    chmod +x ./devops/fix-postgres-private-fqdn.sh
    ./devops/fix-postgres-private-fqdn.sh
  env:
    ARM_CLIENT_ID: ${{ secrets.ARM_CLIENT_ID }}
    ARM_CLIENT_SECRET: ${{ secrets.ARM_CLIENT_SECRET }}
    ARM_SUBSCRIPTION_ID: ${{ secrets.ARM_SUBSCRIPTION_ID }}
    ARM_TENANT_ID: ${{ secrets.ARM_TENANT_ID }}
```

### Azure DevOps
Add this task after Terraform apply:

```yaml
- task: AzureCLI@2
  displayName: 'Fix PostgreSQL Private FQDN'
  inputs:
    azureSubscription: 'your-service-connection'
    scriptType: 'bash'
    scriptLocation: 'scriptPath'
    scriptPath: './devops/fix-postgres-private-fqdn.sh'
```

### Manual Execution
```bash
# Make script executable
chmod +x ./devops/fix-postgres-private-fqdn.sh

# Run the fix
./devops/fix-postgres-private-fqdn.sh
```

## Configuration
The script supports parameters for dynamic environments:
```bash
./fix-postgres-private-fqdn.sh [environment_name] [resource_group] [dns_zone] [key_vault] [secret_name]
```

Default values:
- `environment_name`: "prod"
- `resource_group`: "${environment_name}-zionet-learning-2025"
- `dns_zone`: "privatelink.postgres.database.azure.com"
- `key_vault`: "teachin-seo-kv"
- `secret_name`: "${environment_name}-postgres-connection"

### Manual Execution Examples
```bash
# For production environment
./devops/fix-postgres-private-fqdn.sh prod

# For development environment  
./devops/fix-postgres-private-fqdn.sh dev

# For custom environment with specific parameters
./devops/fix-postgres-private-fqdn.sh \
  "staging" \
  "staging-zionet-learning-2025" \
  "privatelink.postgres.database.azure.com" \
  "teachin-seo-kv" \
  "staging-postgres-connection"
```

## Verification
After running the script:
1. Check Key Vault secret has correct FQDN
2. Verify Kubernetes secret is updated
3. Test application connectivity

## Alternative: Terraform External Data Source
For advanced users, you can modify the Terraform module to use an external data source, but this approach is more complex and requires careful error handling.
