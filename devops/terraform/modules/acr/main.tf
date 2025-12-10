# Azure Container Registry Module

# Create resource group for ACR
resource "azurerm_resource_group" "acr" {
  name     = var.resource_group_name
  location = var.location

  tags = var.tags
}

resource "azurerm_container_registry" "acr" {
  name                = var.acr_name
  resource_group_name = azurerm_resource_group.acr.name
  location            = azurerm_resource_group.acr.location
  sku                 = var.sku
  admin_enabled       = var.admin_enabled

  tags = var.tags

  lifecycle {
    ignore_changes = [tags]
  }

  depends_on = [azurerm_resource_group.acr]
}