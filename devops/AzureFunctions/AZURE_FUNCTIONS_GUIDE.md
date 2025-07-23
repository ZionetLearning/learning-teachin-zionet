# Azure Functions Multi-App Architecture - Best Practices Implementation

## 🎯 **Overview**
This implementation follows Azure best practices for managing multiple Azure Functions using Terraform. It provides:
- **Scalable Architecture**: Easily add/remove function apps
- **Environment Separation**: Different configurations for dev/prod
- **Security Best Practices**: Managed Identity, proper CORS, tagging
- **Maintainability**: DRY principle, reusable modules

## 🏗️ **Architecture**

```
├── modules/
│   └── azure-functions/
│       ├── main.tf        # Multiple function apps with for_each
│       ├── variables.tf   # Standardized input variables
│       └── outputs.tf     # Useful outputs for each function app
├── environments/
│   ├── dev/
│   │   ├── main.tf        # Dev-specific configuration
│   │   └── variables.tf   # Dev function app definitions
│   └── prod/
│       ├── main.tf        # Prod-specific configuration
│       └── variables.tf   # Prod function app definitions
```

## ✅ **Current Function Apps**
- **Accessor**: Data access layer functions
- **Manager**: Business logic management functions  
- **Engine**: Background processing functions

## 🚀 **How to Add New Function Apps**

### **Step 1: Update Environment Variables**
Add your new function app to the `function_apps_config` in both dev and prod:

**Dev Environment** (`environments/dev/variables.tf`):
```terraform
variable "function_apps_config" {
  # ... existing configuration ...
  default = {
    # ... existing function apps ...
    
    # ADD NEW FUNCTION APP HERE
    notifications = {
      name                     = "fa-notifications"
      cors_allowed_origins     = ["http://localhost:3000", "https://localhost:3000"]
      cors_support_credentials = true
      app_settings            = {
        "NOTIFICATION_PROVIDER" = "dev_email"
        "MAX_RETRY_COUNT"      = "3"
      }
      function_type           = "notifications"
      environment             = "dev"
    }
  }
}
```

**Production Environment** (`environments/prod/variables.tf`):
```terraform
variable "function_apps_config" {
  # ... existing configuration ...
  default = {
    # ... existing function apps ...
    
    # ADD NEW FUNCTION APP HERE
    notifications = {
      name                     = "fa-notifications"
      cors_allowed_origins     = ["https://yourdomain.com"]
      cors_support_credentials = false
      app_settings            = {
        "NOTIFICATION_PROVIDER" = "sendgrid"
        "MAX_RETRY_COUNT"      = "5"
      }
      function_type           = "notifications"
      environment             = "prod"
    }
  }
}
```

### **Step 2: Deploy**
```powershell
# Navigate to your environment
cd devops/AzureFunctions/infra/environments/dev

# Plan the changes
terraform plan

# Apply the changes
terraform apply
```

That's it! The new function app will be created automatically.

## 🛡️ **Security Features Implemented**

### **1. Managed Identity**
- Each function app gets a system-assigned managed identity
- Use for secure access to other Azure services
- No credential management needed

### **2. Environment-Specific CORS**
- **Dev**: Allows localhost origins for development
- **Prod**: Restricted to production domains only
- **Credentials**: Enabled in dev, disabled in prod

### **3. Proper Tagging**
- Environment identification
- Resource management
- Cost tracking
- Function type classification

## 📊 **Outputs Available**
The module provides these outputs for each function app:
- `function_app_ids`: Resource IDs
- `function_app_urls`: Default URLs
- `function_app_hostnames`: Hostnames
- `function_app_identity_principal_ids`: Managed identity IDs

## 🎛️ **Per-Function Configuration**
Each function app can have:
- **Custom CORS settings**
- **Specific app settings/environment variables**
- **Individual scaling configuration**
- **Different monitoring settings**

## 📝 **Configuration Examples**

### **API Function App**
```terraform
apis = {
  name                     = "fa-apis"
  cors_allowed_origins     = ["https://frontend.yourdomain.com"]
  cors_support_credentials = false
  app_settings            = {
    "API_VERSION"          = "v1"
    "RATE_LIMIT_REQUESTS"  = "1000"
    "CACHE_DURATION"       = "300"
  }
  function_type           = "api"
  environment             = "prod"
}
```

### **Background Jobs Function App**
```terraform
jobs = {
  name                     = "fa-jobs"
  cors_allowed_origins     = []  # No CORS needed for background jobs
  cors_support_credentials = false
  app_settings            = {
    "JOB_QUEUE_NAME"       = "background-jobs"
    "MAX_CONCURRENT_JOBS"  = "10"
    "JOB_TIMEOUT_MINUTES"  = "30"
  }
  function_type           = "background"
  environment             = "prod"
}
```

### **Event Processing Function App**
```terraform
events = {
  name                     = "fa-events"
  cors_allowed_origins     = ["https://dashboard.yourdomain.com"]
  cors_support_credentials = true
  app_settings            = {
    "EVENT_HUB_CONNECTION" = "EventHubConnectionString"
    "BATCH_SIZE"          = "100"
    "PROCESSING_TIMEOUT"  = "60"
  }
  function_type           = "events"
  environment             = "prod"
}
```

## 🔧 **Best Practices Followed**

1. **Infrastructure as Code**: All configuration in Terraform
2. **Environment Separation**: Different settings per environment
3. **DRY Principle**: Reusable module for all function apps
4. **Security First**: Managed identity, proper CORS, no hardcoded secrets
5. **Monitoring Ready**: Tags and identity for logging/monitoring
6. **Scalable Design**: Easy to add/remove function apps
7. **Cost Optimization**: Shared App Service Plan

## 🚨 **Important Notes**

- All function apps share the same App Service Plan (cost-efficient)
- Each function app gets its own storage container
- Managed identities are automatically created
- Update production CORS origins with your actual domains
- Use Azure Key Vault for sensitive configuration values
