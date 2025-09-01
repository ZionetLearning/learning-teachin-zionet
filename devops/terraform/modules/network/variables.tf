variable "vnet_name" { type = string }
variable "address_space" { type = list(string) }
variable "location" { type = string }
variable "resource_group_name" { type = string }

variable "aks_subnet_name" { type = string }
variable "aks_subnet_prefix" { type = string }
variable "db_subnet_name" { type = string }
variable "db_subnet_prefix" { type = string }
variable "integration_subnet_name" { type = string }
variable "integration_subnet_prefix" { type = string }
variable "management_subnet_name" { type = string }
variable "management_subnet_prefix" { type = string }
