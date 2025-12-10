# Create resource group for Azure AI Foundry
resource "azurerm_resource_group" "foundry" {
  name     = var.resource_group_name
  location = var.location

  tags = var.tags
}

# Azure AI Foundry (Cognitive Services Account with AIServices kind)
resource "azurerm_cognitive_account" "foundry" {
  name                = var.foundry_name
  location            = azurerm_resource_group.foundry.location
  resource_group_name = azurerm_resource_group.foundry.name
  kind                = "AIServices"
  sku_name            = "S0"

  # Disable local auth to use Entra ID authentication
  local_auth_enabled = false

  # Custom subdomain for DNS names
  custom_subdomain_name = var.foundry_name

  tags = var.tags

  lifecycle {
    ignore_changes = [tags]
  }

  depends_on = [azurerm_resource_group.foundry]
}