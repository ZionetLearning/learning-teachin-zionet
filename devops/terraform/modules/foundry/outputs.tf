output "resource_group_id" {
  description = "ID of the Azure AI Foundry resource group"
  value       = azurerm_resource_group.foundry.id
}

output "resource_group_name" {
  description = "Name of the Azure AI Foundry resource group"
  value       = azurerm_resource_group.foundry.name
}

output "foundry_id" {
  description = "ID of the Azure AI Foundry resource"
  value       = azurerm_cognitive_account.foundry.id
}

output "foundry_name" {
  description = "Name of the Azure AI Foundry resource"
  value       = azurerm_cognitive_account.foundry.name
}

output "foundry_endpoint" {
  description = "Endpoint URL of the Azure AI Foundry resource"
  value       = azurerm_cognitive_account.foundry.endpoint
}

output "foundry_key" {
  description = "Primary access key of the Azure AI Foundry resource"
  value       = azurerm_cognitive_account.foundry.primary_access_key
  sensitive   = true
}