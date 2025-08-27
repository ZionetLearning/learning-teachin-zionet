variable "name" {
  type = string
}

variable "location" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "capacity" {
  type    = number
  default = 0
}

variable "family" {
  type    = string
  default = "C"
}

variable "sku_name" {
  type    = string
  default = "Basic"
}

variable "shard_count" {
  type    = number
  default = 0
}

variable "allowed_subnet" {
  type    = string
  default = null
}

# Whether to use a shared Redis instance (true for prod, test, etc.; false for dev)
variable "use_shared_redis" {
  type        = bool
  default     = false
  description = "If true, use a shared Redis instance across environments."
}
