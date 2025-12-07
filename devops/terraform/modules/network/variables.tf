variable "resource_group_name" {
  type        = string
  description = "Name of the resource group"
}

variable "location" {
  type        = string
  description = "Azure region for the virtual network"
}

variable "vnet_name" {
  type        = string
  description = "Name of the main virtual network"
}

variable "address_space" {
  type        = list(string)
  description = "Address space for the main VNet (e.g., [\"10.10.0.0/16\"])"
}

variable "aks_subnet_name" {
  type        = string
  description = "Name of the AKS subnet"
}

variable "aks_subnet_prefix" {
  type        = string
  description = "CIDR for the AKS subnet (e.g., \"10.10.1.0/24\")"
}

variable "enable_db_vnet" {
  type        = bool
  default     = false
  description = "Enable separate database VNet in different region (phase 2). Set true to create DB VNet with peering."
}

variable "db_vnet_name" {
  type        = string
  default     = "teachin-db-vnet"
  description = "Name of the database VNet (used only if enable_db_vnet=true)"
}

variable "db_vnet_location" {
  type        = string
  default     = "Israel Central"
  description = "Location for the database VNet (used only if enable_db_vnet=true)"
}

variable "db_vnet_address_space" {
  type        = list(string)
  default     = ["10.100.0.0/16"]
  description = "Address space for the database VNet (used only if enable_db_vnet=true)"
}

variable "db_subnet_name" {
  type        = string
  default     = "db-subnet"
  description = "Name of the database subnet"
}

variable "db_subnet_prefix" {
  type        = string
  default     = "10.10.2.0/24"
  description = "CIDR for the database subnet"
}

variable "dns_servers" {
  type        = list(string)
  default     = []
  description = "Custom DNS servers for the VNet. Leave empty to use Azure default."
}

variable "tags" {
  type        = map(string)
  default     = {}
  description = "Tags to apply to all resources"
}
