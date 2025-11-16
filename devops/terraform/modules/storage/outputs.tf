output "storage_resource_group_name" {
  description = "Name of the shared storage resource group"
  value       = data.azurerm_resource_group.storage.name
}

output "avatars_storage_account_name" {
  description = "Name of the avatars storage account"
  value       = azurerm_storage_account.avatars.name
}

output "avatars_storage_account_primary_endpoint" {
  description = "Primary blob endpoint for the avatars storage account"
  value       = azurerm_storage_account.avatars.primary_blob_endpoint
}

output "avatars_container_name" {
  description = "Name of the avatars container"
  value       = azurerm_storage_container.avatars.name
}

output "connection_string" {
  description = "Primary connection string of the avatars storage account"
  value       = azurerm_storage_account.avatars.primary_connection_string
  sensitive   = true
}