output "resource_group_id" {
  description = "ID of the Azure Communication Services resource group"
  value       = azurerm_resource_group.email_communication.id
}

output "resource_group_name" {
  description = "Name of the Azure Communication Services resource group"
  value       = azurerm_resource_group.email_communication.name
}

output "communication_service_id" {
  description = "ID of the Azure Communication Service"
  value       = azurerm_communication_service.main.id
}

output "communication_service_name" {
  description = "Name of the Azure Communication Service"
  value       = azurerm_communication_service.main.name
}

output "communication_service_connection_string" {
  description = "Primary connection string for the Communication Service"
  value       = azurerm_communication_service.main.primary_connection_string
  sensitive   = true
}

output "communication_service_key" {
  description = "Primary access key for the Communication Service"
  value       = azurerm_communication_service.main.primary_key
  sensitive   = true
}