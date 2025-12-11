output "resource_group_id" {
  description = "ID of the Key Vault resource group"
  value       = var.create_resource_group ? azurerm_resource_group.keyvault[0].id : null
}

output "resource_group_name" {
  description = "Name of the Key Vault resource group"
  value       = var.create_resource_group ? azurerm_resource_group.keyvault[0].name : var.resource_group_name
}

output "key_vault_id" {
  description = "ID of the Azure Key Vault"
  value       = azurerm_key_vault.main.id
}

output "key_vault_name" {
  description = "Name of the Azure Key Vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI of the Azure Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

output "tenant_id" {
  description = "Tenant ID of the Key Vault"
  value       = azurerm_key_vault.main.tenant_id
}