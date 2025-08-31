# Frontend Module Outputs

output "static_web_app_id" {
  description = "ID of the Azure Static Web App"
  value       = azurerm_static_web_app.frontend.id
}

output "static_web_app_url" {
  description = "Default hostname of the Azure Static Web App"
  value       = azurerm_static_web_app.frontend.default_host_name
}

output "static_web_app_api_key" {
  description = "API key for the Azure Static Web App"
  value       = azurerm_static_web_app.frontend.api_key
  sensitive   = true
}

output "application_insights_id" {
  description = "ID of the Application Insights resource"
  value       = azurerm_application_insights.frontend.id
}

output "application_insights_app_id" {
  value = azurerm_application_insights.frontend.app_id
  description = "Use this in Grafana Azure Monitor data source"
}

output "application_insights_instrumentation_key" {
  description = "Instrumentation key for Application Insights"
  value       = azurerm_application_insights.frontend.instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Connection string for Application Insights"
  value       = azurerm_application_insights.frontend.connection_string
  sensitive   = true
}
