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

# Application Insights outputs
output "application_insights_connection_string" {
  description = "Connection string for Application Insights"
  value       = azurerm_application_insights.frontend.connection_string
  sensitive   = true
}

output "application_insights_instrumentation_key" {
  description = "Instrumentation key for Application Insights"
  value       = azurerm_application_insights.frontend.instrumentation_key
  sensitive   = true
}

output "application_insights_name" {
  description = "Name of the Application Insights resource"
  value       = azurerm_application_insights.frontend.name
}
