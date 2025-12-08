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
  default     = 4
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
  default     = 1
  description = "Minimum number of spot instance nodes"
}

variable "spot_max_node_count" {
  type        = number
  default     = 1
  description = "Maximum number of spot instance nodes"
}

variable "spot_vm_size" {
  type        = string
  default     = "Standard_A2m_v2"
  description = "VM size for spot instance nodes"
}

variable "spot_max_price" {
  type        = number
  default     = 0.0486
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

variable "enable_spot_nodes" {
  type        = bool
  description = "Whether to create spot node pool (disabled for production)"
  default     = true
}

variable "aks_subnet_id" {
  type        = string
  description = "Subnet ID where AKS nodes will be placed"
}

variable "enable_private_cluster" {
  type        = bool
  description = "Enable private cluster (API server private endpoint)"
  default     = true
}

variable "private_dns_zone_id" {
  type        = string
  description = "Private DNS Zone ID for the AKS API server (null = system-managed)"
  default     = null
}

variable "enable_public_fqdn" {
  type        = bool
  description = "Expose the AKS public API FQDN alongside the private endpoint"
  default     = false
}

variable "api_server_authorized_ip_ranges" {
  type        = list(string)
  description = "CIDR list allowed to reach the AKS API server when public FQDN is enabled"
  default     = []
}
