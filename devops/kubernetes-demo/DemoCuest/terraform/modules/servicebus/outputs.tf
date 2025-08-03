output "namespace_id" {
  description = "Service Bus namespace resource ID"
  value       = azurerm_servicebus_namespace.this.id
}

output "namespace_name" {
  description = "Service Bus namespace name"
  value       = azurerm_servicebus_namespace.this.name
}

output "connection_string" {
  description = "Primary connection string for the 'app' authorization rule"
  value       = azurerm_servicebus_namespace_authorization_rule.app.primary_connection_string
}

output "connection_string_base64" {
  description = "Same connection string, Base-64 encoded (handy for some K8s secrets)"
  value       = base64encode(azurerm_servicebus_namespace_authorization_rule.app.primary_connection_string)
}

output "queue_ids" {
  description = "Map of queue resource IDs (key = queue name)"
  value       = { for q in azurerm_servicebus_queue.this : q.name => q.id }
}

output "topic_ids" {
  description = "Map of topic resource IDs (key = topic name)"
  value       = { for t in azurerm_servicebus_topic.this : t.name => t.id }
}
