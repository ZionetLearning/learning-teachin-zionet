output "aks_cluster_name" {
  value = module.aks.cluster_name
}

output "aks_resource_group" {
  value = module.aks.resource_group_name
}

output "aks_kube_config" {
  value     = module.aks.kube_config
  sensitive = true
}

# Optionally output host/certs for use in providers or scripts
output "aks_host" {
  value     = module.aks.kube_config.host
  sensitive = true
}

output "aks_client_certificate" {
  value     = module.aks.client_certificate
  sensitive = true
}
output "aks_client_key" {
  value     = module.aks.client_key
  sensitive = true
}
output "aks_cluster_ca_certificate" {
  value     = module.aks.cluster_ca_certificate
  sensitive = true
}

# Example: output namespace for reference
output "namespace_model" {
  value = kubernetes_namespace.model.metadata[0].name
}

# Connection strings for application deployment
output "servicebus_connection_string" {
  value     = module.servicebus.connection_string
  sensitive = true
}

output "postgres_connection_string" {
  value     = module.database.postgres_connection_string
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
