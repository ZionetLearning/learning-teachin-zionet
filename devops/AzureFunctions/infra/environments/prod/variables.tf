variable "resource_group_name" {
    description = "Name of the resource group"
    type        = string
}

variable "location" {
    description = "Azure region for the resources"
    type        = string
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