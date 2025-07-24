variable "app_service_plan_name" {
  type        = string
  description = "The name of the App Service Plan."
}

variable "location" {
  type        = string
  description = "The location of the App Service Plan."
}

variable "resource_group_name" {
  type        = string
  description = "The name of the resource group in which to create the App Service Plan."
}

variable "sku" {
  type = object({
    tier = string
    size = string
  })
  description = "The SKU of the App Service Plan."
}
