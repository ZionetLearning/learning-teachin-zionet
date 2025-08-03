#------------- General Variables -------------
variable "resource_group_name" {
  description = "Name of the resource group"
  type = string
}
variable "location" {
  description = "Azure region"
  type = string
}
variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
}

variable "tenant_id" {
  description = "Azure tenant ID"
  type        = string
}

#------------- AKS Variables -------------
variable "aks_cluster_name" {
  description = "Name of the AKS cluster"
  type = string
}


variable "node_count" {
  description = "Number of nodes in the AKS cluster"
  type    = number
  default = 2
}
variable "vm_size" {
  description = "Size of the VM instances in the AKS cluster"
  type    = string
  default = "Standard_B2s"
}

#------------- Service Bus Variables -------------
variable "servicebus_namespace" {
  description = "Globally unique Service Bus namespace name"
  type = string
}
variable "servicebus_sku" {
  description = "Service Bus namespace SKU (Basic, Standard, Premium)"
  type    = string
  default = "Standard"
}
variable "queue_names" {
  description = "List of Service Bus queues to create"
  type = list(string)
  default = [
              "manager-to-engine",
              "taskupdate",
              "engine-to-accessor",
              "taskupdate-input"
            ]
}

#------------- Docker Hub (or ACR) Variables --------------------
variable "docker_registry" {
  description = "Container registry/org used in deployment YAMLs"
  type        = string
}

#------------- SignalR Service Variables -------------
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

#------------- Postgres Variables -------------

variable "database_server_name" {
  description = "Name of the PostgreSQL server (must be globally unique)"
  type        = string
}

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

variable "grafana_admin_user" {
  description = "Grafana admin username"
  type        = string
  default     = "admin"
}

variable "grafana_admin_password" {
  description = "Grafana admin password"
  type        = string
  default     = "admin123"
}

variable "grafana_namespace" {
  description = "Namespace for Grafana"
  type        = string
  default     = "devops-logs"
}

variable "grafana_storage_class" {
  description = "Storage class for Grafana PVC"
  type        = string
  default     = ""
}

# mc_resource_group
variable "mc_resource_group" {
  description = "Resource group for the managed cluster"
  type        = string
  default = "MC_dev-zionet-learning-2025_aks-cluster-dev_westeurope"
}