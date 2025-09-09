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
  value     = var.use_shared_postgres ? format("Host=%s;Database=%s;Username=%s;Password=%s;SslMode=Require", data.azurerm_postgresql_flexible_server.shared[0].fqdn, "${var.database_name}-${var.environment_name}", var.admin_username, var.admin_password) : module.database[0].postgres_connection_string
  sensitive = true
}

output "signalr_connection_string" {
  value     = module.signalr.primary_connection_string
  sensitive = true
}


output "redis_hostname" {
  value = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].hostname : module.redis[0].hostname
}

output "redis_primary_access_key" {
  value     = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : module.redis[0].primary_access_key
  sensitive = true
}

# Frontend outputs (conditional based on frontend_apps array)
output "static_web_app_urls" {
  description = "URLs of the Azure Static Web Apps"
  value       = length(var.frontend_apps) > 0 ? { for app_name, frontend in module.frontend : app_name => frontend.static_web_app_url } : {}
}

output "static_web_app_api_keys" {
  description = "API keys for the Azure Static Web Apps"
  value       = length(var.frontend_apps) > 0 ? { for app_name, frontend in module.frontend : app_name => frontend.static_web_app_api_key } : {}
  sensitive   = true
}

output "application_insights_connection_strings" {
  description = "Connection strings for Application Insights per frontend app"
  value       = length(var.frontend_apps) > 0 ? { for app_name, frontend in module.frontend : app_name => frontend.application_insights_connection_string } : {}
  sensitive   = true
}

output "frontend_apps_enabled" {
  description = "List of enabled frontend applications"
  value       = var.frontend_apps
}
