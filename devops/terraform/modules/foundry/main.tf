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

  depends_on = [azurerm_resource_group.foundry]
}

# Azure Speech Service
resource "azurerm_cognitive_account" "speech" {
  name                = var.speech_service_name
  location            = azurerm_resource_group.foundry.location
  resource_group_name = azurerm_resource_group.foundry.name
  kind                = "SpeechServices"
  sku_name            = "F0"

  # Enable local auth to allow API key access
  local_auth_enabled = true


  depends_on = [azurerm_resource_group.foundry]
}