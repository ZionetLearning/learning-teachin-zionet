variable "resource_group_name" {
  type        = string
  description = "Name of the resource group"
}

variable "location" {
  type        = string
  description = "Azure region"
}

variable "signalr_name" {
  type        = string
  description = "Globally unique SignalR name"
}

variable "sku_name" {
  type        = string
  default     = "Standard_S1"
  description = "SignalR SKU (e.g. Free_F1, Standard_S1)"
}

variable "sku_capacity" {
  type        = number
  default     = 1
  description = "Instance count for SignalR"
}