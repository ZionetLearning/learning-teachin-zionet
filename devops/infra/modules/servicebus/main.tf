############################################
# Azure Service Bus Namespace + Queues + Topics
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

resource "azurerm_servicebus_queue" "this" {
  for_each            = toset(var.queue_names)

  name                = each.value
  namespace_id = azurerm_servicebus_namespace.this.id

  max_size_in_megabytes = 1024
}

resource "azurerm_servicebus_topic" "this" {
  for_each            = toset(var.topic_names)
  name                = each.value
  namespace_id = azurerm_servicebus_namespace.this.id
}
resource "azurerm_servicebus_subscription" "this" {
  for_each = merge([
    for topic, subs in var.topic_subscriptions :
    {
      for sub in subs :
      "${topic}/${sub}" => {
        topic = topic
        name  = sub
      }
    }
  ]...)

  name                = each.value.name
  topic_id            = azurerm_servicebus_topic.this[each.value.topic].id
  max_delivery_count  = 1
}
############################################
# END
############################################
