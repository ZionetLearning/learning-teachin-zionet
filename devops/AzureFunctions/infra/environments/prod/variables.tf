variable "resource_group_name" {
    description = "Name of the resource group"
    type        = string
}

variable "location" {
    description = "Azure region for the resources"
    type        = string
}

variable "db_location" {
    description = "Location for the PostgreSQL database"
    type        = string
    default     = "Israel Central"
  
}

# db_version
variable "db_version" {
  type        = string
  description = "PostgreSQL version"
}

variable "servicebus_namespace_name" {
    description = "Name of the Service Bus Namespace"
    type        = string
}

# storage_account_name
variable "storage_account_name" {
    description = "Name of the Storage Account"
    type        = string
}

# storage_account_tier 
variable "storage_account_tier" {
    description = "Tier of the Storage Account (e.g., Standard, Premium)"
    type        = string
}

#storage_account_replication_type
variable "storage_account_replication_type" {
    description = "Replication type of the Storage Account (e.g., LRS, GRS, ZRS)"
    type        = string
}

# app_service_plan_sku
variable "app_service_plan_sku" {
    description = "SKU for the App Service Plan"
    type = object({
        tier = string
        size = string
    })
}



# cosmos_account_name
variable "cosmos_account_name" {
  description = "Name of the Cosmos DB account"
  type        = string
}

# database_name
variable "database_name" {
  description = "Name of the Mongo database"
  type        = string
}

# var.offer_type
variable "offer_type" {
  description = "Offer type for the Cosmos DB account (e.g., Standard)"
  type        = string
}

# var.kind
variable "kind" {
  description = "Kind of the Cosmos DB account (e.g., MongoDB, GlobalDocumentDB)"
  type        = string
}

# consistency_policy
variable "consistency_policy" {
  description = "Consistency policy for the Cosmos DB account"
  type        = object({
    consistency_level = string
  })
}

# failover_priority
variable "failover_priority" {
  description = "Failover priority for the Cosmos DB account"
  type        = number
}

# capabilities
variable "capabilities" {
  description = "Capabilities for the Cosmos DB account"
  type        = list(object({
    name = string
  }))
}

# PostgreSQL database variables

variable "admin_username" {
  type        = string
  description = "PostgreSQL administrator username"
}

variable "admin_password" {
  type        = string
  sensitive   = true
  description = "PostgreSQL administrator password"
}

variable "version" {
  type        = string
  description = "PostgreSQL version"
}

variable "sku_name" {
  type        = string
  description = "PostgreSQL SKU (pricing tier)"
}

variable "storage_mb" {
  type        = number
  description = "PostgreSQL storage size in MB"
}

variable "password_auth_enabled" {
  type        = bool
  description = "Enable password-based authentication"
}

variable "active_directory_auth_enabled" {
  type        = bool
  description = "Enable Active Directory authentication"
}

variable "backup_retention_days" {
  type        = number
  description = "Backup retention period in days"
}

variable "geo_redundant_backup_enabled" {
  type        = bool
  description = "Enable geo-redundant backups"
}

variable "high_availability_mode" {
  type        = string
  description = "High availability mode"
}

variable "delegated_subnet_id" {
  type        = string
  description = "Delegated subnet ID (for VNet integration)"
}

variable "database_name" {
  type        = string
  description = "Name of the PostgreSQL database"
}

#------------------- Azure Functions -------------------

# Multiple Function Apps Configuration
variable "function_apps_config" {
  type = map(object({
    name                     = string
    cors_allowed_origins     = list(string)
    cors_support_credentials = bool
    app_settings            = map(string)
    function_type           = string
    environment             = string
  }))
  description = "Configuration for multiple function apps"
  default = {
    accessor = {
      name                     = "fa-accessor"
      cors_allowed_origins     = ["https://yourdomain.com", "https://www.yourdomain.com"]
      cors_support_credentials = false
      app_settings            = {
        "ACCESSOR_SPECIFIC_SETTING" = "accessor_prod_value"
      }
      function_type           = "accessor"
      environment             = "prod"
    }
    manager = {
      name                     = "fa-manager"
      cors_allowed_origins     = ["https://yourdomain.com", "https://www.yourdomain.com"]
      cors_support_credentials = false
      app_settings            = {
        "MANAGER_SPECIFIC_SETTING" = "manager_prod_value"
      }
      function_type           = "manager"
      environment             = "prod"
    }
    engine = {
      name                     = "fa-engine"
      cors_allowed_origins     = ["https://yourdomain.com", "https://www.yourdomain.com"]
      cors_support_credentials = false
      app_settings            = {
        "ENGINE_SPECIFIC_SETTING" = "engine_prod_value"
      }
      function_type           = "engine"
      environment             = "prod"
    }
  }
}

variable "common_tags" {
  type        = map(string)
  description = "Common tags for all resources"
  default = {
    "Environment" = "prod"
    "Project"     = "learning-teachin-zionet"
    "ManagedBy"   = "terraform"
  }
}
