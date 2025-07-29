#------------------  Resource Group ------------------
variable "resource_group_name" {
  description = "Name of the Azure resource group"
  type = string
}

variable "location" {
  description = "Azure region where resources will be created"
  type = string
}

#------------------  AKS ------------------
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
  description = "Size of the virtual machines in the AKS cluster"
  type    = string
  default = "Standard_B2s"
}



#------------------  Service Bus ------------------
variable "servicebus_namespace" {
  description = "Name of the Service Bus namespace"
  type = string
}
variable "queue_names" {
  description = "List of queue names to create in the Service Bus namespace"
  type = list(string)
  default = [
              "clientcallback",
              "clientresponsequeue",
              "todomanagercallbackqueue",
              "todoqueue"
            ]
}

variable "servicebus_sku" {
  description = "SKU for the Service Bus namespace"
  type    = string
  default = "Standard"
}


# ------------------  Docker Hub (or ACR) ------------------
# Docker Hub (or ACR) org that prefixes every image URL
variable "docker_registry" {
  description = "Container registry/org used in deployment YAMLs"
  type        = string
}

#------------------  SignalR ------------------
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