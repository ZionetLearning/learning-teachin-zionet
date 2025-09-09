############################################
# Azure Service Bus Namespace + Queues
# Author: 2025-07-22
############################################

resource "azurerm_servicebus_namespace" "this" {
  name                = var.namespace_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku          # eg "Standard", "Basic", "Premium"

  tags = var.tags
}

# Namespace-level shared-access policy for your app(s)
resource "azurerm_servicebus_namespace_authorization_rule" "app" {
  name                = "app"
  namespace_id = azurerm_servicebus_namespace.this.id

  listen  = true
  send    = true
  manage  = true
}

# Optional – create queues from list
resource "azurerm_servicebus_queue" "this" {
  for_each            = toset(var.queue_names)

  name                = each.value
  namespace_id = azurerm_servicebus_namespace.this.id

  max_size_in_megabytes = 1024
  requires_session     = contains(var.session_enabled_queues, each.value)
}

# Optional – create topics from list
resource "azurerm_servicebus_topic" "this" {
  for_each            = toset(var.topic_names)

  name                = each.value
  namespace_id = azurerm_servicebus_namespace.this.id
}

############################################
# END
############################################
