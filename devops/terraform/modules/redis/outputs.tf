# Conditional outputs based on shared vs dedicated Redis
output "hostname" {
  description = "Redis hostname (from dedicated or shared instance)"
  value       = var.use_shared_redis ? var.shared_redis_hostname : azurerm_redis_cache.this[0].hostname
}

output "port" {
  description = "Redis port (from dedicated or shared instance)"
  value       = var.use_shared_redis ? var.shared_redis_port : azurerm_redis_cache.this[0].port
}

output "ssl_port" {
  description = "Redis SSL port (from dedicated or shared instance)"
  value       = var.use_shared_redis ? var.shared_redis_ssl_port : azurerm_redis_cache.this[0].ssl_port
}

output "primary_access_key" {
  description = "Redis primary access key (from dedicated or shared instance)"
  value       = var.use_shared_redis ? var.shared_redis_primary_access_key : azurerm_redis_cache.this[0].primary_access_key
  sensitive   = true
}

output "database_index" {
  description = "Redis database index for this environment"
  value       = var.database_index
}

output "use_shared_redis" {
  description = "Whether this module is using shared Redis"
  value       = var.use_shared_redis
}

# Connection string output for convenience
output "connection_string" {
  description = "Redis connection string with database index"
  value       = "${var.use_shared_redis ? var.shared_redis_hostname : azurerm_redis_cache.this[0].hostname}:${var.use_shared_redis ? var.shared_redis_ssl_port : azurerm_redis_cache.this[0].ssl_port},password=${var.use_shared_redis ? var.shared_redis_primary_access_key : azurerm_redis_cache.this[0].primary_access_key},ssl=True,abortConnect=False,database=${var.database_index}"
  sensitive   = true
}
