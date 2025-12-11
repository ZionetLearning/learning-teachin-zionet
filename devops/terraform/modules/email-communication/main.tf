# Create resource group for Azure Communication Services
resource "azurerm_resource_group" "email_communication" {
  name     = var.resource_group_name
  location = var.location
}

# Azure Communication Service
resource "azurerm_communication_service" "main" {
  name                = var.communication_service_name
  resource_group_name = azurerm_resource_group.email_communication.name
  data_location       = var.data_location

  depends_on = [azurerm_resource_group.email_communication]
}