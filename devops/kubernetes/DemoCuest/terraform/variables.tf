variable "location" {
  type = string
}
variable "resource_group_name" {
  type = string
}
variable "aks_cluster_name" {
  type = string
}
variable "servicebus_namespace" {
  type = string
}
variable "queue_names" {
  type = list(string)
  default = [
              "manager-to-engine",
              "taskupdate",
              "engine-to-accessor",
              "taskupdate-input"
            ]
}
# variable "cosmosdb_account_name" {
#   type = string
# }
variable "node_count" {
  type    = number
  default = 2
}
variable "vm_size" {
  type    = string
  default = "Standard_B2s"
}
variable "sku" {
  type    = string
  default = "Standard"
}

variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
}

variable "tenant_id" {
  description = "Azure tenant ID"
  type        = string
}

# Docker Hub (or ACR) org that prefixes every image URL
variable "docker_registry" {
  description = "Container registry/org used in deployment YAMLs"
  type        = string
}

# variable "cosmosdb_sql_database_name" {
#   type    = string
#   default = "ToDoDatabase"
# }

# variable "cosmosdb_sql_container_name" {
#   type    = string
#   default = "ToDos"
# }

# variable "cosmosdb_partition_key_path" {
#   type    = string
#   default = "/id"
# }

variable "signalr_name" {
  type        = string
  description = "SignalR service name (must be globally unique)"
  default     = "signalRdemoCuest"
}

variable "signalr_sku_name" {
  type        = string
  default     = "Free_F1"
}

variable "signalr_sku_capacity" {
  type        = number
  default     = 1
}



# PostgreSQL database variables

variable "db_location" {
    description = "Location for the PostgreSQL database"
    type        = string
    default     = "Israel Central"
  
}


# admin_username
variable "admin_username" {
  type        = string
  description = "PostgreSQL administrator username"
  default = "postgres"
}

# admin_password
variable "admin_password" {
  type        = string
  sensitive   = true
  description = "PostgreSQL administrator password"
  default     = "postgres"
}

# db_version
variable "db_version" {
  type        = string
  description = "PostgreSQL version"
  default     = "15"
}

# sku_name
variable "sku_name" {
  type        = string
  description = "SKU name for the PostgreSQL server"
  default = "B_Standard_B1ms"
}

# storage_mb
variable "storage_mb" {
  type        = number
  description = "Storage size in MB for the PostgreSQL server"
  default = 32768
}

# password_auth_enabled
variable "password_auth_enabled" {
  type        = bool
  description = "Enable password authentication for PostgreSQL"
  default = true
}

# active_directory_auth_enabled
variable "active_directory_auth_enabled" {
  type        = bool
  description = "Enable Active Directory authentication for PostgreSQL"
  default = false
}

# backup_retention_days
variable "backup_retention_days" {
  type        = number
  description = "Number of days to retain backups for PostgreSQL"
  default = 7
}

# geo_redundant_backup_enabled
variable "geo_redundant_backup_enabled" {
  type        = bool
  description = "Enable geo-redundant backups for PostgreSQL"
  default = false
}


# delegated_subnet_id
variable "delegated_subnet_id" {
  type        = string
  description = "ID of the delegated subnet for PostgreSQL"
  default     = null
}

# database_name
variable "database_name" {
  type        = string
  description = "Name of the PostgreSQL database to create"
  default     = "appdb-dev"
}
