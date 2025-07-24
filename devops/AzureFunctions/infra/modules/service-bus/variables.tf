variable "queues" {
  description = "Map of queue names and their configurations"
  type = map(object({
    max_delivery_count     = optional(number)
    default_message_ttl    = optional(string)
    enable_dead_lettering  = optional(bool)
    enable_partitioning    = optional(bool)
    max_size_in_megabytes = optional(number)
  }))
  default = {}
}

variable "namespace_name" {
  description = "Name of the Service Bus Namespace"
  type        = string
}

variable "location" {
  description = "Azure region for the Service Bus Namespace"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group for the Service Bus Namespace"
  type        = string
}
variable "sku" {
  description = "SKU for the Service Bus Namespace"
  type        = string
  default     = "Standard"
}