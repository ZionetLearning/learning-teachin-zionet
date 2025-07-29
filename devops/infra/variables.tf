#------------- General Variables -------------
variable "resource_group_name" {
  type = string
}
variable "location" {
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

#------------- Service Bus Variables -------------
variable "servicebus_sku" {
  type    = string
  default = "Standard"
}

variable "servicebus_namespace" {
  type = string
}
variable "queue_names" {
  type = list(string)
  default = [
              # "clientcallback",
              # "clientresponsequeue",
              # "todomanagercallbackqueue",
              # "todoqueue"
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