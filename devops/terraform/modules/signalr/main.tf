resource "azurerm_signalr_service" "this" {
  name                = var.signalr_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku {
    name     = var.sku_name   # Free_F1, Standard_S1, etc.
    capacity = var.sku_capacity
  }
}