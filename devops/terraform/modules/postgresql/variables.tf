variable "server_name" {
  type        = string
  description = "Name of the PostgreSQL flexible server"
}

variable "location" {
  type        = string
  description = "Azure region"
}

variable "resource_group_name" {
  type        = string
  description = "Name of the resource group"
}

variable "admin_username" {
  type        = string
  description = "PostgreSQL administrator username"
}

variable "admin_password" {
  type        = string
  sensitive   = true
  description = "Administrator password (sensitive)"
}

variable "db_version" {
  type        = string
  description = "PostgreSQL version"
  default     = "16"
}

variable "sku_name" {
  type        = string
  description = "SKU name for pricing tier (e.g., B1ms)"
  default     = "B_Standard_B1ms"
}

variable "storage_mb" {
  type        = number
  description = "Storage size in MB"
  default     = 32768
}

variable "backup_retention_days" {
  type        = number
  description = "Number of days to retain backups"
  default     = 7
}

variable "geo_redundant_backup_enabled" {
  type        = bool
  description = "Enable geo-redundant backup (only for supported SKUs)"
  default     = false
}

variable "password_auth_enabled" {
  type        = bool
  description = "Enable password authentication"
  default     = true
}

variable "active_directory_auth_enabled" {
  type        = bool
  description = "Enable Active Directory authentication"
  default     = false
}

variable "delegated_subnet_id" {
  type        = string
  description = "Delegated subnet ID if using VNet integration"
}

variable "database_name" {
  type        = string
  description = "Name of the PostgreSQL database"
}

# Use shared postgres server logic (like use_shared_aks)
variable "use_shared_postgres" {
  type        = bool
  description = "If true, use shared postgres server; if false, create new server."
  default     = true
}
# Environment name to control dynamic server/database creation
variable "environment_name" {
  type        = string
  description = "Name of the environment (e.g., dev, test, prod, feature-123)"
}

# Existing server ID for non-dev environments
variable "existing_server_id" {
  type        = string
  description = "ID of the existing PostgreSQL flexible server to use for non-dev environments"
  default     = ""
}