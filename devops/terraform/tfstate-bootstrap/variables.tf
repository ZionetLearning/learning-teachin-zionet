variable "resource_group_name" {
  description = "The name of the Resource Group for the Terraform state storage."
  type        = string
  default     = "teachin-tfstate-rg"
}

variable "storage_account_name" {
  description = "The globally unique name of the Storage Account for the Terraform state."
  type        = string
  default     = "tfstateteachin"
}

variable "container_name" {
  description = "The name of the Storage Container for the Terraform state."
  type        = string
  default     = "tfstate-aks"
}

variable "location" {
  description = "The Azure region where the resources should be created."
  type        = string
  default     = "West Europe"
}

variable "account_tier" {
  description = "The tier of the Storage Account."
  type        = string
  default     = "Standard"
}

variable "account_replication_type" {
  description = "The replication type of the Storage Account."
  type        = string
  default     = "LRS"
}

variable "min_tls_version" {
  description = "The minimum TLS version for the Storage Account."
  type        = string
  default     = "TLS1_2"
}

variable "is_hns_enabled" {
  description = "Specifies whether Hierarchical Namespace is enabled for the Storage Account."
  type        = bool
  default     = false
}

variable "container_access_type" {
  description = "The access type of the Storage Container."
  type        = string
  default     = "private"
}
