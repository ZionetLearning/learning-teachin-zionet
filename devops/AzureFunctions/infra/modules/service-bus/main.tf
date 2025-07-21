resource "azurerm_servicebus" "name" {
  name                = var.namespace_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku

  tags = {
    CreatedBy = "Terraform"
  }
}

# Multiple Service Bus Queues
resource "azurerm_servicebus_queue" "this" {
  for_each     = var.queues
  name         = each.key
  namespace_id = azurerm_servicebus_namespace.this.id

  # Queue settings with defaults
  max_delivery_count               = lookup(each.value, "max_delivery_count", 10) # attempts before moving to dead-letter
  default_message_ttl              = lookup(each.value, "default_message_ttl", "P14D") # Time To Live, P14D = ISO 8601 duration format = 14 days
  dead_lettering_on_message_expiration = lookup(each.value, "enable_dead_lettering", true) 
  max_size_in_megabytes           = lookup(each.value, "max_size_in_megabytes", 1024) # Size of the queue
}