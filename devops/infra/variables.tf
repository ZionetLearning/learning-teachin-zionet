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
variable "db_location" {
    description = "Location for the PostgreSQL database"
    type        = string
    default     = "Israel Central"
}

variable "admin_username" {
  description = "Admin username for the PostgreSQL server"
  type        = string
}
variable "admin_password" {
  description = "Admin password for the PostgreSQL server"
  type        = string
  sensitive   = true
}
# db_version
variable "db_version" {
  type        = string
  description = "PostgreSQL version"
}

# sku_name
variable "sku_name" {
  type        = string
  description = "SKU name for the PostgreSQL server"
}
#storage_mb
variable "storage_mb" {
 

variable "db_username" {
  description = "Username for the PostgreSQL database"
  type        = string
}

variable "db_password" {
  description = "Password for the PostgreSQL database"
  type        = string
  sensitive   = true
}

variable "db_name" {
  description = "Name of the PostgreSQL database"
  type        = string
}

