variable "storage_account_name" {
  type        = string
  description = "The name of the storage account."
  
}

variable "resource_group_name" {
  type        = string
  description = "The name of the resource group in which to create the storage account."
}

variable "location" {
  type        = string
  description = "The location of the storage account."
}

variable "account_tier" {
  type        = string
  description = "The account tier of the storage account (e.g., Standard, Premium)."
}

variable "account_replication_type" {
  type        = string
  description = "The replication type of the storage account (e.g., LRS, GRS, ZRS)."
}