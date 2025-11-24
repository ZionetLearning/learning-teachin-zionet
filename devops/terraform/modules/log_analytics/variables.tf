variable "environment_name" {
    description = "Name of the environment (e.g., dev, staging, prod, feature-123)"
    type        = string
}

variable "resource_group_name" {
  type        = string
}

variable "location" {
  type        = string
}

variable "sku" {
  description = "SKU for the Log Analytics Workspace"
  type        = string
  default     = "PerGB2018"
}

variable "retention_in_days" {
  description = "Retention period for the logs (in days)"
  type        = number
  default     = 30
}

variable "daily_quota_gb" {
  description = "Daily quota for the logs (in GB)"
  type        = number
  default     = 1
}