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


variable "location" {
  type        = string
  default     = "East US"
  description = "Azure region"
}

# PostgreSQL database variables

# admin_username
variable "admin_username" {
  type        = string
  description = "PostgreSQL administrator username"
}

# admin_password
variable "admin_password" {
  type        = string
  sensitive   = true
  description = "PostgreSQL administrator password"
}

# version
variable "version" {
  type        = string
  description = "PostgreSQL version"
}

# sku_name
variable "sku_name" {
  type        = string
  description = "SKU name for the PostgreSQL server"
}

# storage_mb
variable "storage_mb" {
  type        = number
  description = "Storage size in MB for the PostgreSQL server"
}

# password_auth_enabled
variable "password_auth_enabled" {
  type        = bool
  description = "Enable password authentication for PostgreSQL"
}

# active_directory_auth_enabled
variable "active_directory_auth_enabled" {
  type        = bool
  description = "Enable Active Directory authentication for PostgreSQL"
}

# backup_retention_days
variable "backup_retention_days" {
  type        = number
  description = "Number of days to retain backups for PostgreSQL"
}

# geo_redundant_backup_enabled
variable "geo_redundant_backup_enabled" {
  type        = bool
  description = "Enable geo-redundant backups for PostgreSQL"
}

# high_availability_mode
variable "high_availability_mode" {
  type        = string
  description = "High availability mode for PostgreSQL (e.g., ZoneRedundant, Disabled)"
}

# delegated_subnet_id
variable "delegated_subnet_id" {
  type        = string
  description = "ID of the delegated subnet for PostgreSQL"
}

# database_name
variable "database_name" {
  type        = string
  description = "Name of the PostgreSQL database to create"
}

