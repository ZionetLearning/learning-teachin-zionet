provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = "sb-queue-test"
  location = "Israel Central"
}

resource "azurerm_servicebus_namespace" "namespace" {
  name                = "sb-dev-shared-queue"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "Basic"
}

resource "azurerm_servicebus_queue" "queue" {
  name                = "incoming-queue"
  namespace_id        = azurerm_servicebus_namespace.namespace.id
}
