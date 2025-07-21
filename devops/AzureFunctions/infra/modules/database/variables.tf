variable "cosmos_account_name" {
  description = "Name of the Cosmos DB account"
  type        = string
}

variable "database_name" {
  description = "Name of the Mongo database"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

# offer_type
variable "offer_type" {
  description = "Offer type for the Cosmos DB account (e.g., Standard)"
  type        = string
}

# kind
variable "kind" {
  description = "Kind of the Cosmos DB account (e.g., MongoDB, GlobalDocumentDB)"
  type        = string
}

# consistency_policy
variable "consistency_policy" {
  description = "Consistency policy for the Cosmos DB account"
  type        = object({
    consistency_level = string
  })
}

# failover_priority
variable "failover_priority" {
  description = "Failover priority for the Cosmos DB account"
  type        = number
}

# capabilities
variable "capabilities" {
  description = "Capabilities for the Cosmos DB account"
  type        = list(object({
    name = string
  }))
}