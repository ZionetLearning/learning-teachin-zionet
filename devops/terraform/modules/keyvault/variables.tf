variable "key_vault_name" {
  description = "Name of the Azure Key Vault"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "West Europe"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "sku_name" {
  description = "SKU name for the Key Vault"
  type        = string
  default     = "standard"
}

variable "soft_delete_retention_days" {
  description = "Number of days to retain soft-deleted keys"
  type        = number
  default     = 90
}

variable "purge_protection_enabled" {
  description = "Enable purge protection for the Key Vault"
  type        = bool
  default     = false
}

variable "network_default_action" {
  description = "Default action for network access"
  type        = string
  default     = "Allow"
}

variable "network_bypass" {
  description = "Bypass network rules for Azure services"
  type        = string
  default     = "AzureServices"
}

variable "allowed_ip_ranges" {
  description = "List of allowed IP ranges for Key Vault access"
  type        = list(string)
  default     = []
}

variable "additional_access_policies" {
  description = "Additional access policies for the Key Vault"
  type = list(object({
    object_id                   = string
    secret_permissions         = list(string)
    key_permissions            = list(string)
    certificate_permissions    = list(string)
  }))
  default = []
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}