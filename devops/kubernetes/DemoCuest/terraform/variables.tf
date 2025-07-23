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
}

variable "tenant_id" {
  description = "Azure tenant ID"
  type        = string
}

# Docker Hub (or ACR) org that prefixes every image URL
variable "docker_registry" {
  description = "Container registry/org used in deployment YAMLs"
  type        = string
}
