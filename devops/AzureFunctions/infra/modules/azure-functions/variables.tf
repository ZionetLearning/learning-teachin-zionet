variable "function_apps" {
  type = map(object({
    name                     = string
    cors_allowed_origins     = list(string)
    cors_support_credentials = bool
    app_settings            = map(string)
    function_type           = string
    environment             = string
  }))
  description = "Map of function apps to create with their configurations"
}

variable "location" {
  type        = string
  description = "The location of the function app."
}

variable "resource_group_name" {
  type        = string
  description = "The name of the resource group in which to create the function app."
}

variable "app_service_plan_id" {
  type        = string
  description = "The ID of the App Service Plan."
}

variable "storage_account_name" {
  type        = string
  description = "The name of the storage account."
}

variable "storage_account_access_key" {
  type        = string
  description = "The access key of the storage account."
}

variable "service_bus_connection_string" {
  type        = string
  description = "The connection string for the Service Bus."
}

variable "common_tags" {
  type        = map(string)
  description = "Common tags to apply to all resources"
  default     = {}
}
