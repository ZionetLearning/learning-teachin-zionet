# Outputs for deployment and configuration
output "resource_group_name" {
  description = "The name of the resource group"
  value       = module.resource_group.name
}

output "function_app_name" {
  description = "The name of the Function App"
  value       = module.function_app.name
}

output "function_app_default_hostname" {
  description = "The default hostname of the Function App"
  value       = module.function_app.default_hostname
}

output "storage_account_name" {
  description = "The name of the storage account"
  value       = module.storage_account.name
}

output "service_bus_namespace_name" {
  description = "The name of the Service Bus namespace"
  value       = module.service_bus.namespace_name
}

output "service_bus_connection_string" {
  description = "The Service Bus connection string"
  value       = module.service_bus.connection_string
  sensitive   = true
}

output "database_fqdn" {
  description = "The FQDN of the PostgreSQL server"
  value       = module.database.postgres_fqdn
}

output "database_connection_string" {
  description = "The PostgreSQL connection string"
  value       = module.database.postgres_connection_string
  sensitive   = true
}
