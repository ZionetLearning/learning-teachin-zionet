# Data source for current client configuration
data "azurerm_client_config" "current" {}

# Create resource group for Key Vault (conditional)
resource "azurerm_resource_group" "keyvault" {
  count    = var.create_resource_group ? 1 : 0
  name     = var.resource_group_name
  location = var.location
}

# Create Key Vault
resource "azurerm_key_vault" "main" {
  name                       = var.key_vault_name
  location                   = var.create_resource_group ? azurerm_resource_group.keyvault[0].location : var.location
  resource_group_name        = var.resource_group_name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = var.sku_name
  soft_delete_retention_days = var.soft_delete_retention_days
  purge_protection_enabled   = var.purge_protection_enabled

  # Network access rules
  network_acls {
    default_action = var.network_default_action
    bypass         = var.network_bypass
    ip_rules       = var.allowed_ip_ranges
  }
}

# Access policy for the current service principal/user
resource "azurerm_key_vault_access_policy" "current_user" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get",
    "List",
    "Set",
    "Delete",
    "Recover",
    "Backup",
    "Restore",
    "Purge"
  ]

  key_permissions = [
    "Get",
    "List",
    "Create",
    "Delete",
    "Recover",
    "Backup", 
    "Restore",
    "Purge",
    "Decrypt",
    "Encrypt",
    "Sign",
    "Verify",
    "WrapKey",
    "UnwrapKey"
  ]

  certificate_permissions = [
    "Get",
    "List",
    "Create",
    "Delete",
    "Recover",
    "Backup",
    "Restore", 
    "Purge",
    "Import",
    "Update"
  ]
}

# Access policies for additional users
resource "azurerm_key_vault_access_policy" "additional_users" {
  count        = length(var.additional_user_object_ids)
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = var.additional_user_object_ids[count.index]

  secret_permissions = [
    "Get",
    "List",
    "Set",
    "Delete",
    "Recover",
    "Backup",
    "Restore",
    "Purge"
  ]

  key_permissions = [
    "Get",
    "List",
    "Create",
    "Delete",
    "Recover",
    "Backup", 
    "Restore",
    "Purge",
    "Decrypt",
    "Encrypt",
    "Sign",
    "Verify",
    "WrapKey",
    "UnwrapKey"
  ]

  certificate_permissions = [
    "Get",
    "List",
    "Create",
    "Delete",
    "Recover",
    "Backup",
    "Restore", 
    "Purge",
    "Import",
    "Update"
  ]
}