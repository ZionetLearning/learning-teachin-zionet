variable "location" {
  description = "Azure region"
  type        = string
  default     = "West Europe"
}

variable "data_location" {
  description = "Data residency location for Communication Services"
  type        = string
  default     = "Europe"
}

variable "communication_service_name" {
  description = "Name of the Azure Communication Service"
  type        = string
  default     = "teachin-comm-svc-test"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "email-service-rg-test"
}

