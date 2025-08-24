# Frontend Module Variables

variable "static_web_app_name" {
  description = "Name of the Azure Static Web App"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "sku_tier" {
  description = "SKU tier for the Static Web App (Free or Standard)"
  type        = string
  default     = "Free"
}

variable "sku_size" {
  description = "SKU size for the Static Web App (Free or Standard)"
  type        = string
  default     = "Free"
}

variable "appinsights_retention_days" {
  description = "Number of days to retain Application Insights data"
  type        = number
  default     = 30
}

variable "appinsights_sampling_percentage" {
  description = "Sampling percentage for Application Insights"
  type        = number
  default     = 100
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
