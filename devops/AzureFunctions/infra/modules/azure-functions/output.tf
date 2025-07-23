output "name" {
  description = "The name of the Function App"
  value       = azurerm_linux_function_app.this.name
}

output "id" {
  description = "The ID of the Function App"
  value       = azurerm_linux_function_app.this.id
}

output "default_hostname" {
  description = "The default hostname of the Function App"
  value       = azurerm_linux_function_app.this.default_hostname
}

output "kind" {
  description = "The kind of the Function App"
  value       = azurerm_linux_function_app.this.kind
}