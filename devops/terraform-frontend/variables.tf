# Variables for the Terraform configuration
variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "static_web_app_name" {
  description = "Name of the static web app"
  type        = string
}

variable "location" {
  description = "Azure region for resources"
  type        = string
}


