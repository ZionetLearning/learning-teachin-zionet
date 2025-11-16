variable "resource_group_name" {
  type = string
}
variable "location" {
  type = string
}
variable "cluster_name" {
  type = string
}

# Stable node pool configuration
variable "stable_min_node_count" {
  type        = number
  default     = 1
  description = "Minimum number of stable nodes (not spot instances)"
}

variable "stable_max_node_count" {
  type        = number
  default     = 2
  description = "Maximum number of stable nodes (not spot instances)"
}

variable "stable_vm_size" {
  type        = string
  default     = "Standard_B2s"
  description = "VM size for stable nodes"
}

# Spot instance node pool configuration
variable "spot_min_node_count" {
  type        = number
  default     = 0
  description = "Minimum number of spot instance nodes"
}

variable "spot_max_node_count" {
  type        = number
  default     = 1
  description = "Maximum number of spot instance nodes"
}

variable "spot_vm_size" {
  type        = string
  default     = "Standard_B2s"
  description = "VM size for spot instance nodes"
}

variable "spot_max_price" {
  type        = number
  default     = -1
  description = "Maximum price for spot instances (-1 = pay up to on-demand price)"
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
