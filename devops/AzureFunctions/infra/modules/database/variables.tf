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

variable "version" {
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

variable "high_availability_mode" {
  type        = string
  description = "High availability mode (e.g., 'Disabled', 'ZoneRedundant')"
}

variable "delegated_subnet_id" {
  type        = string
  description = "Delegated subnet ID if using VNet integration"
}

variable "database_name" {
  type        = string
  description = "Name of the PostgreSQL database"
}