output "foundry_id" {
  description = "ID of the AI Foundry"
  value       = azurerm_ai_foundry.foundry.id
}

output "foundry_name" {
  description = "Name of the AI Foundry"
  value       = azurerm_ai_foundry.foundry.name
}

output "storage_account_id" {
  description = "ID of the storage account"
  value       = azurerm_storage_account.foundry.id
}

output "key_vault_id" {
  description = "ID of the Key Vault"
  value       = azurerm_key_vault.foundry.id
}