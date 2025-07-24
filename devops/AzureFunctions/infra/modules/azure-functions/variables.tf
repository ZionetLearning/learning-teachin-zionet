variable "function_app_name" {
  type        = string
  description = "The name of the function app."
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
