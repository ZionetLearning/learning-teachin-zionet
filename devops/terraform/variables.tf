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
  description = "List of frontend applications to deploy. Set to [] to disable Static Web Apps creation."
  type        = list(string)
  default     = ["student", "teacher", "admin"]
}

#------------- Langfuse Variables -------------
variable "enable_langfuse" {
  description = "Enable Langfuse database creation (only applies to dev environment)"
  type        = bool
  default     = true
}