output "aks_cluster_name" {
  value = local.aks_cluster_name
}

output "aks_resource_group" {
  value = local.aks_resource_group
}

output "aks_kube_config" {
  value     = local.aks_kube_config
  sensitive = true
}

# Optionally output host/certs for use in providers or scripts
output "aks_host" {
  value     = local.aks_kube_config.host
  sensitive = true
}

output "aks_client_certificate" {
  value     = local.aks_kube_config.client_certificate
  sensitive = true
}
output "aks_client_key" {
  value     = local.aks_kube_config.client_key
  sensitive = true
}
output "aks_cluster_ca_certificate" {
  value     = local.aks_kube_config.cluster_ca_certificate
  sensitive = true
}

# Environment-specific namespace outputs
output "kubernetes_namespace" {
  value = kubernetes_namespace.environment.metadata[0].name
}

output "environment_name" {
  value = var.environment_name
}

output "use_shared_aks" {
  value = var.use_shared_aks
}

# Connection strings for application deployment
output "servicebus_connection_string" {
  value     = module.servicebus.connection_string
  sensitive = true
}

output "postgres_connection_string" {
  value = var.use_shared_postgres ? format("Host=%s;Database=%s;Username=%s;Password=%s;SslMode=Require", data.azurerm_postgresql_flexible_server.shared[0].fqdn, "${var.database_name}-${var.environment_name}", var.admin_username, var.admin_password) : module.database[0].postgres_connection_string
  sensitive = true
}

output "signalr_connection_string" {
  value     = module.signalr.primary_connection_string
  sensitive = true
}

output "redis_hostname" {
  value = module.redis.hostname
}
output "redis_primary_access_key" {
  value     = module.redis.primary_access_key
  sensitive = true
}

# Frontend outputs
output "static_web_app_url" {
  description = "URL of the Azure Static Web App"
  value       = module.frontend.static_web_app_url
}

output "static_web_app_api_key" {
  description = "API key for the Azure Static Web App"
  value       = module.frontend.static_web_app_api_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Connection string for Application Insights"
  value       = module.frontend.application_insights_connection_string
  sensitive   = true
}

# Custom Domain outputs (CNAME only)
output "frontend_custom_domain_enabled" {
  description = "Whether custom domain is enabled for frontend"
  value       = module.frontend.custom_domain_enabled
}

output "frontend_custom_domain_name" {
  description = "Custom domain name if configured for frontend"
  value       = module.frontend.custom_domain_name
}

output "frontend_cname_target" {
  description = "CNAME target for DNS configuration (point your domain to this)"
  value       = module.frontend.cname_target
}

output "frontend_final_url" {
  description = "Final URL to use for frontend - custom domain if available, otherwise default URL"
  value       = module.frontend.final_url
}
