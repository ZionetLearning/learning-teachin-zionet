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
              "clientcallback",
              "clientresponsequeue",
              "todomanagercallbackqueue",
              "todoqueue"
            ]
}
variable "cosmosdb_account_name" {
  type = string
}
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
  default     = ""
}

variable "tenant_id" {
  description = "Azure tenant ID"
  type        = string
  default     = ""
}

# Docker Hub (or ACR) org that prefixes every image URL
variable "docker_registry" {
  description = "Container registry/org used in deployment YAMLs"
  type        = string
}

variable "cosmosdb_sql_database_name" {
  type    = string
  default = "ToDoDatabase"
}

variable "cosmosdb_sql_container_name" {
  type    = string
  default = "ToDos"
}

variable "cosmosdb_partition_key_path" {
  type    = string
  default = "/id"
}

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