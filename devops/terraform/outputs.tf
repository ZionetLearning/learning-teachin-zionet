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
  value     = module.database.postgres_connection_string
  sensitive = true
}

output "signalr_connection_string" {
  value     = module.signalr.primary_connection_string
  sensitive = true
}

output "redis_hostname" {
  description = "Redis hostname"
  value       = module.redis.hostname
}

output "redis_port" {
  description = "Redis port"
  value       = module.redis.port
}

output "redis_ssl_port" {
  description = "Redis SSL port"
  value       = module.redis.ssl_port
}

output "redis_primary_access_key" {
  description = "Redis primary access key"
  value       = module.redis.primary_access_key
  sensitive   = true
}

output "redis_database_index" {
  description = "Redis database index for this environment"
  value       = module.redis.database_index
}

output "redis_connection_string" {
  description = "Redis connection string with database index"
  value       = module.redis.connection_string
  sensitive   = true
}

output "use_shared_redis" {
  description = "Whether shared Redis is being used"
  value       = module.redis.use_shared_redis
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
