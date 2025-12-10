# Azure AI Foundry Module

# Storage Account for AI Foundry (required)
resource "azurerm_storage_account" "foundry" {
  name                     = "${replace(var.foundry_name, "-", "")}storage"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  
  tags = var.tags
}

# Key Vault for AI Foundry
resource "azurerm_key_vault" "foundry" {
  name                = "${var.foundry_name}-kv"
  location            = var.location
  resource_group_name = var.resource_group_name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  tags = var.tags
}

# Get current Azure client configuration
data "azurerm_client_config" "current" {}

# AI Foundry resource
resource "azurerm_ai_foundry" "foundry" {
  name                = var.foundry_name
  location            = var.location
  resource_group_name = var.resource_group_name
  storage_account_id  = azurerm_storage_account.foundry.id
  key_vault_id        = azurerm_key_vault.foundry.id

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags

  lifecycle {
    ignore_changes = [tags]
  }
}