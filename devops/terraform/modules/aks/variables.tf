variable "resource_group_name" {
  type = string
}
variable "location" {
  type = string
}
variable "cluster_name" {
  type = string
}
variable "node_count" {
  type    = number
  default = 2
}
variable "max_node_count" {
  type    = number
  default = 3
}
variable "min_node_count" {
  type    = number
  default = 1
}
variable "vm_size" {
  type    = string
  default = "Standard_B2s"
}

variable "identity_type" {
  description = "The type of identity to assign to the AKS cluster (SystemAssigned or UserAssigned)"
  type        = string
  default     = "SystemAssigned"
}

variable "identity_ids" {
  description = "Optional list of UserAssigned Identity IDs to use if type is UserAssigned"
  type        = list(string)
  default     = []
}

variable "prefix" {
  type        = string
  description = "Prefix for naming resources"
  default     = "dev" # or whatever you want
}
