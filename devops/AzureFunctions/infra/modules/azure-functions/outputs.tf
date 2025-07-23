output "function_app_ids" {
  description = "Map of function app names to their IDs"
  value       = { for k, v in azurerm_linux_function_app.this : k => v.id }
}

output "function_app_urls" {
  description = "Map of function app names to their default URLs"
  value       = { for k, v in azurerm_linux_function_app.this : k => "https://${v.name}.azurewebsites.net" }
}

output "function_app_hostnames" {
  description = "Map of function app names to their hostnames"
  value       = { for k, v in azurerm_linux_function_app.this : k => v.default_hostname }
}

output "function_app_identity_principal_ids" {
  description = "Map of function app names to their managed identity principal IDs"
  value       = { for k, v in azurerm_linux_function_app.this : k => v.identity[0].principal_id }
}
