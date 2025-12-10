output "foundry_id" {
  description = "ID of the AI Foundry"
  value       = azurerm_ai_foundry.foundry.id
}

output "foundry_endpoint" {
  description = "Endpoint URL of the AI Foundry"
  value       = azurerm_ai_foundry.foundry.endpoint
}

output "foundry_key" {
  description = "Primary access key of the AI Foundry"
  value       = azurerm_ai_foundry.foundry.primary_access_key
  sensitive   = true
}

output "storage_account_id" {
  description = "ID of the storage account"
  value       = azurerm_storage_account.foundry.id
}

output "key_vault_id" {
  description = "ID of the Key Vault"
  value       = azurerm_key_vault.foundry.id
}