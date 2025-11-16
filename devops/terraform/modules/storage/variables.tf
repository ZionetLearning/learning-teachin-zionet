variable "name" {
    description = "Name of the storage account"
    type        = string
    default = "storage-rg"
}

variable "environment_name" {
    description = "Name of the environment (e.g., dev, staging, prod, feature-123)"
    type        = string
}

variable "account_tier" {
    description = "Account tier for the storage account"
    type        = string
    default     = "Standard"
}

variable "account_replication_type" {
    description = "Replication type for the storage account"
    type        = string
    default     = "LRS"
}
variable "access_tier" {
    description = "Access tier for the storage account"
    type        = string
    default     = "Cool"
}

variable "allow_nested_items_to_be_public" {
    description = "Allow nested items to be public"
    type        = bool
    default     = true
}

variable "min_tls_version" {
    description = "Minimum TLS version for the storage account"
    type        = string
    default     = "TLS1_2"
}

variable "https_traffic_only_enabled" {
    description = "Enable HTTPS traffic only"
    type        = bool
    default     = true
}

variable "days" {
    description = "Number of days for retention policies"
    type        = number
    default     = 7
}

variable "versioning_enabled" {
    description = "Enable versioning for the storage account"
    type        = bool
    default     = false
}

variable "container_access_type" {
    description = "Access type for the storage container"
    type        = string
    default     = "private"
}

variable "enabled" {
    description = "Enable lifecycle management for the storage account"
    type        = bool
    default     = true
}

variable "tier_to_cool_after_days_since_modification_greater_than" {
    description = "Number of days after which to move blobs to Cool tier"
    type        = number
    default     = 30
}

variable "tier_to_archive_after_days_since_modification_greater_than" {
    description = "Number of days after which to move blobs to Archive tier"
    type        = number
    default     = 90
}