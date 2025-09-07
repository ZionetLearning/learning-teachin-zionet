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
}

variable "sku_name" {
  type        = string
  description = "SKU name for pricing tier (e.g., B1ms)"
}

variable "storage_mb" {
  type        = number
  description = "Storage size in MB"
}

variable "backup_retention_days" {
  type        = number
  description = "Number of days to retain backups"
}

variable "geo_redundant_backup_enabled" {
  type        = bool
  description = "Enable geo-redundant backup (only for supported SKUs)"
}

variable "password_auth_enabled" {
  type        = bool
  description = "Enable password authentication"
}

variable "active_directory_auth_enabled" {
  type        = bool
  default     = false
  description = "Enable Active Directory authentication"
}

# variable "high_availability_mode" {
#   type        = string
#   description = "High availability mode (e.g., 'Disabled', 'ZoneRedundant')"
#   default     = ""
# }

variable "db_subnet_id" {
  type        = string
  description = "ID of the dedicated database subnet for PostgreSQL VNet integration"
  default     = null
}

variable "database_name" {
  type        = string
  description = "Name of the PostgreSQL database"
}

# variable "aks_public_ip" {
#   type        = string
#   description = "Public IP of the AKS cluster"
# }

# Use shared postgres server logic (like use_shared_aks)
variable "use_shared_postgres" {
  type        = bool
  description = "If true, use shared postgres server; if false, create new server."
  default     = true
}
# Environment name to control dynamic server/database creation


# Existing server ID for non-dev environments
variable "existing_server_id" {
  type        = string
  description = "ID of the existing PostgreSQL flexible server to use for non-dev environments"
  default     = ""
}

variable "virtual_network_id" {
  type        = string
  description = "ID of the virtual network for Private DNS Zone linking"
  default     = null
}

