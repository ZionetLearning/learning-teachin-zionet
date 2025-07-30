# variable "resource_group_name" {
#   type = string
# }
# variable "location" {
#   type = string
# }
# variable "cluster_name" {
#   type = string
# }
# variable "node_count" {
#   type    = number
#   default = 2
# }
# variable "vm_size" {
#   type    = string
#   default = "Standard_B2s"
# }


# modules/aks/variables.tf

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "cluster_name" {
  description = "Name of the AKS cluster"
  type        = string
}

variable "node_count" {
  description = "Number of nodes in the default node pool"
  type        = number
  default     = 2
}

variable "vm_size" {
  description = "Size of the VMs in the default node pool"
  type        = string
  default     = "Standard_B2s"
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}