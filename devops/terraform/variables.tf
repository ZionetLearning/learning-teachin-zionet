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

variable "identity_id" {
  description = "Azure AD identity ID"
  type        = string
  default     = "0997f44d-fadf-4be8-8dc6-202f7302f680"
}

variable "tenant_id" {
  description = "Azure tenant ID"
  type        = string
  default     = "a814ee32-f813-4a36-9686-1b9268183e27"
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

#------------- Service Bus Variables -------------
variable "servicebus_namespace" {
  description = "Globally unique Service Bus namespace name"
  type        = string
  default     = "servicebus-zionet-learning"
}

variable "queue_names" {
  description = "List of Service Bus queues to create"
  type        = list(string)
  default = [
    "manager-callback-queue",
    "engine-queue",
    "accessor-queue",
    "manager-callback-session-queue",
  ]
}

variable "session_enabled_queues" {
  description = "List of queues that require session support"
  type        = list(string)
  default     = ["manager-callback-session-queue"]
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

#------------- Postgres Variables -------------
variable "use_shared_postgres" {
  description = "Use shared PostgreSQL server instead of creating new one"
  type        = bool
  default     = true
}
variable "database_server_name" {
  description = "Name of the PostgreSQL server (must be globally unique)"
  type        = string
  default     = "pg-zionet-learning"
}

variable "db_location" {
  description = "Location for the PostgreSQL database"
  type        = string
  default     = "Israel Central"
}

# communication_service_connection_string
variable "communication_service_connection_string" {
  type        = string
  sensitive   = true
  description = "Azure Communication Service connection string"
  default     = null
}

# tavily_api_key
variable "tavily_api_key" {
  type        = string
  sensitive   = true
  description = "Tavily API key for search functionality"
  default     = null
}


# admin_username - passed from GitHub Actions as TF_VAR_admin_username
variable "admin_username" {
  type        = string
  description = "PostgreSQL administrator username - provided by GitHub Actions"
}

# admin_password - passed from GitHub Actions as TF_VAR_admin_password  
variable "admin_password" {
  type        = string
  sensitive   = true
  description = "PostgreSQL administrator password - provided by GitHub Actions"
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

variable "frontend_apps" {
  description = "List of frontend applications to deploy. Set to [] to disable Static Web Apps creation."
  type        = list(string)
  default     = ["student", "teacher", "admin"]
}

#------------- Redis Variables -------------
variable "redis_name" {
  description = "Name of the Redis cache instance (for dev/test environments)"
  type        = string
  default     = "redis-teachin-shared"
}

variable "use_shared_redis" {
  description = "Use shared Redis instance instead of creating new one"
  type        = bool
  default     = true
}

variable "shared_redis_name" {
  type        = string
  default     = null
  description = "Name of shared Redis cache, if using shared"
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

#------------- Langfuse Variables -------------
variable "enable_langfuse" {
  description = "Enable Langfuse database creation (only applies to dev environment)"
  type        = bool
  default     = true
}