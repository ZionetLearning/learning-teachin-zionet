# Azure OpenAI Module

# Azure OpenAI Service
resource "azurerm_cognitive_account" "openai" {
  name                = var.foundry_name
  location            = var.location
  resource_group_name = var.resource_group_name
  kind                = "OpenAI"
  sku_name            = "S0"

  tags = var.tags

  lifecycle {
    ignore_changes = [tags]
  }
}