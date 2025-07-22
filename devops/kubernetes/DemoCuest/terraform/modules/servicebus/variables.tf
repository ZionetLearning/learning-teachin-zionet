############################################
# Inputs
############################################

variable "resource_group_name" {
  description = "Existing resource-group name"
  type        = string
}

variable "location" {
  description = "Azure location (e.g. West Europe)"
  type        = string
}

variable "namespace_name" {
  description = "Globally unique Service Bus namespace name"
  type        = string
}

variable "sku" {
  description = "Namespace SKU (Basic, Standard, Premium)"
  type        = string
  default     = "Standard"
}

variable "queue_names" {
  description = "List of queues to create (leave empty if you create queues elsewhere)"
  type        = list(string)
  default     = []
}

variable "topic_names" {
  description = "List of topics to create"
  type        = list(string)
  default     = []
}

variable "tags" {
  description = "Key/value map of tags applied to all resources"
  type        = map(string)
  default     = {}
}
