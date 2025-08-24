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

variable "database_index" {
  description = "Redis database index for this environment (0-15)"
  type        = number
  default     = 0
  validation {
    condition     = var.database_index >= 0 && var.database_index <= 15
    error_message = "Database index must be between 0 and 15."
  }
}

variable "use_shared_redis" {
  description = "Whether this module is being used for shared Redis configuration"
  type        = bool
  default     = false
}

variable "shared_redis_hostname" {
  description = "Hostname of shared Redis instance (when use_shared_redis is true)"
  type        = string
  default     = null
}

variable "shared_redis_port" {
  description = "Port of shared Redis instance (when use_shared_redis is true)"
  type        = number
  default     = null
}

variable "shared_redis_ssl_port" {
  description = "SSL port of shared Redis instance (when use_shared_redis is true)"
  type        = number
  default     = null
}

variable "shared_redis_primary_access_key" {
  description = "Primary access key of shared Redis instance (when use_shared_redis is true)"
  type        = string
  default     = null
  sensitive   = true
}
