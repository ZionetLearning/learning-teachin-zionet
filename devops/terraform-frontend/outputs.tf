# Output values
output "resource_group_name" {
  description = "Name of the created resource group"
  value       = azurerm_resource_group.frontend.name
}

output "static_web_app_name" {
  description = "Name of the created static web app"
  value       = azurerm_static_web_app.frontend.name
}

output "static_web_app_url" {
  description = "URL of the static web app"
  value       = "https://${azurerm_static_web_app.frontend.default_host_name}"
}

output "deployment_token" {
  description = "Deployment token for the static web app"
  value       = azurerm_static_web_app.frontend.api_key
  sensitive   = true
}

output "resource_group_id" {
  description = "ID of the created resource group"
  value       = azurerm_resource_group.frontend.id
}

output "static_web_app_id" {
  description = "ID of the created static web app"
  value       = azurerm_static_web_app.frontend.id
}
