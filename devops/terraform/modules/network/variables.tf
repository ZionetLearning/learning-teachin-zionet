# Network Module Variables
# This module handles VNet, subnets, NSGs, and private endpoints

#--------------------- VNet Variables ---------------------
variable "vnet_name" {
  description = "Name of the Virtual Network"
  type        = string
}

variable "address_space" {
  description = "Address space for the Virtual Network"
  type        = list(string)
  validation {
    condition     = length(var.address_space) > 0
    error_message = "Address space must contain at least one CIDR block."
  }
}

variable "location" {
  description = "Azure region for the network resources"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group where network resources will be created"
  type        = string
}

#--------------------- Subnet Variables ---------------------
variable "aks_subnet_name" {
  description = "Name of the AKS subnet"
  type        = string
}

variable "aks_subnet_prefix" {
  description = "Address prefix for the AKS subnet"
  type        = string
  validation {
    condition     = can(cidrhost(var.aks_subnet_prefix, 0))
    error_message = "AKS subnet prefix must be a valid CIDR block."
  }
}

variable "db_subnet_name" {
  description = "Name of the database subnet"
  type        = string
}

variable "db_subnet_prefix" {
  description = "Address prefix for the database subnet"
  type        = string
  validation {
    condition     = can(cidrhost(var.db_subnet_prefix, 0))
    error_message = "Database subnet prefix must be a valid CIDR block."
  }
}

variable "integration_subnet_name" {
  description = "Name of the integration subnet"
  type        = string
}

variable "integration_subnet_prefix" {
  description = "Address prefix for the integration subnet"
  type        = string
  validation {
    condition     = can(cidrhost(var.integration_subnet_prefix, 0))
    error_message = "Integration subnet prefix must be a valid CIDR block."
  }
}

variable "management_subnet_name" {
  description = "Name of the management subnet"
  type        = string
}

variable "management_subnet_prefix" {
  description = "Address prefix for the management subnet"
  type        = string
  validation {
    condition     = can(cidrhost(var.management_subnet_prefix, 0))
    error_message = "Management subnet prefix must be a valid CIDR block."
  }
}

#--------------------- Optional Variables ---------------------
variable "tags" {
  description = "Tags to apply to network resources"
  type        = map(string)
  default = {
    ManagedBy = "terraform"
  }
}

variable "enable_ddos_protection" {
  description = "Enable DDoS protection on the VNet"
  type        = bool
  default     = false
}

variable "dns_servers" {
  description = "List of DNS servers for the VNet"
  type        = list(string)
  default     = []
}
