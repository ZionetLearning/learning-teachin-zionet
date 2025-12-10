variable "foundry_name" {
  description = "Name of the Azure AI Foundry resource"
  type        = string
  default     = "teachin-foundry"
}

variable "speech_service_name" {
  description = "Name of the Azure Speech service"
  type        = string
  default     = "teachin-speech"
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "France Central"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "azure-open-ai-playground-rg"
}

