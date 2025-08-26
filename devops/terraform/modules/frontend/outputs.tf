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

# Custom Domain Outputs (CNAME only)
output "custom_domain_enabled" {
  description = "Whether custom domain is enabled"
  value       = var.custom_domain_enabled
}

output "custom_domain_name" {
  description = "Custom domain name if configured"
  value       = var.custom_domain_enabled ? var.custom_domain_name : null
}

output "cname_target" {
  description = "CNAME target for DNS configuration (the default hostname without https://)"
  value       = var.custom_domain_enabled ? azurerm_static_web_app.frontend.default_host_name : null
}

output "final_url" {
  description = "Final URL to use - custom domain if available, otherwise default URL"
  value       = var.custom_domain_enabled ? "https://${var.custom_domain_name}" : "https://${azurerm_static_web_app.frontend.default_host_name}"
}
