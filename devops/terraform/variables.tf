#------------- General Variables -------------
variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "zionet-learning-2025"
}
variable "location" {
  description = "Azure region"
  type        = string
}
variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
}
variable "tenant_id" {
  description = "Azure tenant ID"
  type        = string
}
variable "shared_resource_group" {
  description = "Resource group containing the shared AKS cluster, PostgreSQL server, and Redis cache"
  type        = string
  default     = "dev-zionet-learning-2025"
}
#------------- AKS Variables -------------
variable "aks_cluster_name" {
  description = "Name of the AKS cluster"
  type        = string
  default     = "aks-cluster-dev"
}
variable "node_count" {
  description = "Number of nodes in the AKS cluster"
  type        = number
  default     = 2
}
variable "vm_size" {
  description = "Size of the VM instances in the AKS cluster"
  type        = string
  default     = "Standard_B2s"
}

#------------- Service Bus Variables -------------
variable "servicebus_namespace" {
  description = "Globally unique Service Bus namespace name"
  type        = string
  default     = "servicebus-zionet-learning"
}
variable "servicebus_sku" {
  description = "Service Bus namespace SKU (Basic, Standard, Premium)"
  type        = string
  default     = "Standard"
}
variable "queue_names" {
  description = "List of Service Bus queues to create"
  type        = list(string)
  default = [
    "manager-callback-queue",
    "engine-queue",
    "accessor-queue",
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
  default     = "signalr-teachin"
}

variable "signalr_sku_name" {
  type    = string
  default = "Free_F1"
}

variable "signalr_sku_capacity" {
  type    = number
  default = 1
}

#------------- Postgres Variables -------------
variable "use_shared_postgres" {
  description = "Use shared PostgreSQL server instead of creating new one"
  type        = bool
  default     = true
}
variable "database_server_name" {
  description = "Name of the PostgreSQL server (must be globally unique)"
  type        = string
  default     = "dev-pg-zionet-learning"
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
  default     = "postgres"
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
  default     = "16"
}
# sku_name
variable "sku_name" {
  type        = string
  description = "SKU name for the PostgreSQL server"
  default     = "B_Standard_B1ms"
}
# storage_mb
variable "storage_mb" {
  type        = number
  description = "Storage size in MB for the PostgreSQL server"
  default     = 32768
}
# password_auth_enabled
variable "password_auth_enabled" {
  type        = bool
  description = "Enable password authentication for PostgreSQL"
  default     = true
}
# active_directory_auth_enabled
variable "active_directory_auth_enabled" {
  type        = bool
  description = "Enable Active Directory authentication for PostgreSQL"
  default     = false
}
# backup_retention_days
variable "backup_retention_days" {
  type        = number
  description = "Number of days to retain backups for PostgreSQL"
  default     = 7
}
# geo_redundant_backup_enabled
variable "geo_redundant_backup_enabled" {
  type        = bool
  description = "Enable geo-redundant backups for PostgreSQL"
  default     = false
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
  default     = "appdb"
}

#------------- Frontend Variables -------------
variable "static_web_app_name" {
  description = "Name of the Azure Static Web App"
  type        = string
  default     = "static-web-app"
}

variable "frontend_sku_tier" {
  description = "SKU tier for the Static Web App (Free or Standard)"
  type        = string
  default     = "Free"
}

variable "frontend_sku_size" {
  description = "SKU size for the Static Web App (Free or Standard)"
  type        = string
  default     = "Free"
}

variable "frontend_appinsights_retention_days" {
  description = "Number of days to retain Application Insights data"
  type        = number
  default     = 30
}

variable "frontend_appinsights_sampling_percentage" {
  description = "Sampling percentage for Application Insights"
  type        = number
  default     = 100
}
#------------- Redis Variables -------------
variable "redis_name" {
  description = "Name of the Redis cache instance"
  type        = string
  default     = "redis-teachin-shared"
}

variable "use_shared_redis" {
  description = "Use shared Redis instance instead of creating new one"
  type        = bool
  default     = true
}

#------------- Environment Variables -------------
variable "environment_name" {
  description = "Name of the environment (e.g., dev, staging, prod, feature-123)"
  type        = string
}

variable "use_shared_aks" {
  description = "Use shared AKS cluster instead of creating new one"
  type        = bool
  default     = true
}

variable "shared_aks_cluster_name" {
  description = "Name of the shared AKS cluster to use"
  type        = string
  default     = "aks-cluster-dev"
}

# Namespace configuration
variable "kubernetes_namespace" {
  description = "Kubernetes namespace for this environment"
  type        = string
  default     = ""
}

variable "prefix" {
  type        = string
  description = "Prefix for naming resources"
  default     = "dev"
}

#------------- Frontend Application Variables -------------
variable "frontend_apps" {
  description = "List of frontend applications to deploy"
  type        = list(string)
  #default     = ["student", "teacher", "admin"]
  default     = [] # Delete after mi is done (and uncomment the line above)
}

variable "workload_sa_bindings" {
  description = "List of SAs to bind via Workload Identity."
  type = list(object({
    namespace      : string
    serviceaccount : string
    name_suffix    : optional(string)
  }))
  default = []
}

variable "servicebus_namespaces" {
  description = "Service Bus namespaces to grant MI to."
  type = list(object({
    name            : string
    resource_group  : string
    assign_sender   : bool
    assign_receiver : bool
  }))
  default = []
}

# Where the AKS cluster actually lives
variable "aks_resource_group_name" {
  type        = string
  description = "Resource group name of the AKS cluster."
  default     = "dev-zionet-learning-2025"
}

# Where the UAMI (workload identity) lives
variable "uami_resource_group_name" {
  type        = string
  description = "Resource group name of the User Assigned Managed Identity used by workloads."
  default     = "dev-zionet-learning-2025"
}

# Optional override; otherwise we derive "<environment_name>-aks-uami"
variable "uami_name" {
  type        = string
  description = "Name of the UAMI used by workloads (optional)."
  default     = null
}


variable "enable_frontend" { # Delete after mi is done
  description = "Whether to create frontend Static Web Apps."
  type        = bool
  default     = false
}

variable "frontend_apps" { # Delete after mi is done
  description = "Frontend apps to create"
  type        = list(string)
  default     = []
}