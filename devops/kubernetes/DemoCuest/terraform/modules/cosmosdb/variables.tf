variable "resource_group_name" {
  type = string
}
variable "location" {
  type = string
}
variable "cosmosdb_account_name" {
  type = string
}

variable "cosmosdb_sql_database_name" {
  type    = string
}

variable "cosmosdb_sql_container_name" {
  type    = string
}

variable "cosmosdb_partition_key_path" {
  type    = string
}
