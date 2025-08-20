# Environment Cleanup Guide

## Overview
This guide explains how to automatically and manually clean up temporary environments while protecting production resources.

## Protected Environments
The following environments are **NEVER** deleted:
- `dev`
- `prod` 
- `production`
- `staging`

## Automatic Cleanup (Recommended)

### Daily Scheduled Cleanup
- **Runs**: Every day at 11 PM UTC
- **Mode**: Dry run by default (shows what would be deleted)
- **Protection**: Built-in safety checks

### Manual Trigger via GitHub Actions
```bash
# Test what would be cleaned up (dry run)
gh workflow run cleanup-environments.yaml

# Actually perform cleanup
gh workflow run cleanup-environments.yaml -f dry_run=false

# Cleanup specific pattern
gh workflow run cleanup-environments.yaml -f environment_pattern=test*
```

## Manual Cleanup Script

### Quick Commands
```bash
# See what would be deleted
./devops/cleanup-temp-envs.sh dry-run

# Actually delete temporary environments
./devops/cleanup-temp-envs.sh force

# Target specific pattern
./devops/cleanup-temp-envs.sh force test*
```

### Make Script Executable
```bash
chmod +x ./devops/cleanup-temp-envs.sh
```

## What Gets Cleaned Up

### Azure Resources
- **Resource Groups**: `{environment-name}-zionet-learning-2025`
- **All contained resources**: PostgreSQL, Service Bus, Redis, SignalR, etc.
- **Exclusions**: Resource groups starting with `dev-`, `prod-`, `production-`, `staging-`

### Kubernetes Resources
- **Namespaces**: Custom namespaces (not system ones)
- **All contained resources**: Deployments, services, pods, secrets, etc.
- **Exclusions**: `kube-*`, `default`, `devops-*`, `dev`, `prod`, `production`, `staging`

## Safety Features

### Multiple Protection Layers
1. **Environment name checking**: Protected environments are hardcoded
2. **Resource group tagging**: Tagged with `Protected=true/false`
3. **Safety validation**: Double-check before deletion
4. **Dry run default**: Always shows what would be deleted first
5. **Manual confirmation**: Requires explicit confirmation for force mode

### Tags Added by Terraform
```hcl
tags = {
  Environment = var.environment_name
  ManagedBy   = "terraform"
  CreatedDate = timestamp()
  Protected   = contains(["dev", "prod", "production", "staging"], var.environment_name) ? "true" : "false"
}
```

## Usage Examples

### Create and Test Temporary Environment
```bash
# Create temporary environment
gh workflow run full-cicd.yaml -f environment_name=test123

# Use the environment for testing...

# Check what cleanup would do
./devops/cleanup-temp-envs.sh dry-run

# Clean up when done
./devops/cleanup-temp-envs.sh force
```

### Scheduled Cleanup for Cost Control
The automated cleanup runs daily and:
- Identifies environments older than 1 day
- Skips protected environments
- Deletes resource groups in background (fast)
- Provides summary of actions taken

## Cost Savings
- **Prevents forgotten environments**: Automatic detection and cleanup
- **Complete resource deletion**: No orphaned resources left behind
- **Background processing**: Fast, non-blocking deletions
- **Typical savings**: 50-80% reduction in test environment costs

## Troubleshooting

### Check What Environments Exist
```bash
# List all resource groups
az group list --query "[?contains(name, 'zionet-learning-2025')].{Name:name, Environment:tags.Environment, Protected:tags.Protected}" -o table

# List all custom namespaces
kubectl get namespaces --show-labels
```

### Manual Recovery
If you accidentally delete something:
```bash
# Recreate dev environment
gh workflow run full-cicd.yaml -f environment_name=dev

# Check resource group tags
az group show --name dev-zionet-learning-2025 --query tags
```

### Disable Automatic Cleanup
```yaml
# In .github/workflows/cleanup-environments.yaml
on:
  # schedule:
  #   - cron: '0 23 * * *'  # Comment out this line
  workflow_dispatch:
    # Manual trigger only
```

## Best Practices

1. **Always test with dry-run first**
2. **Use descriptive environment names** (e.g., `test-feature-xyz`)
3. **Don't use protected names** for temporary environments
4. **Check costs regularly** in Azure portal
5. **Set up budget alerts** for unexpected costs
6. **Document long-running test environments** if they need to persist
