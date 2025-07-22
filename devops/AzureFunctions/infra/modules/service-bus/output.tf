output "namespace_id" {
  description = "The ID of the Service Bus namespace"
  value       = azurerm_servicebus_namespace.this.id
}

output "namespace_name" {
  description = "The name of the Service Bus namespace"
  value       = azurerm_servicebus_namespace.this.name
}

output "primary_connection_string" {
  description = "The primary connection string for the Service Bus namespace"
  value       = azurerm_servicebus_namespace.this.default_primary_connection_string
  sensitive   = true
}

output "secondary_connection_string" {
  description = "The secondary connection string for the Service Bus namespace"
  value       = azurerm_servicebus_namespace.this.default_secondary_connection_string
  sensitive   = true
}

output "queue_ids" {
  description = "Map of queue names to their IDs"
  value       = { for k, v in azurerm_servicebus_queue.this : k => v.id }
}

output "queue_names" {
  description = "List of created queue names"
  value       = [for queue in azurerm_servicebus_queue.this : queue.name]
}

output "connection_string" {
  description = "The primary connection string for the Service Bus namespace (alias for primary_connection_string)"
  value       = azurerm_servicebus_namespace.this.default_primary_connection_string
  sensitive   = true
}

