
output "hostname" {
  value = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].hostname : azurerm_redis_cache.this[0].hostname
}

output "port" {
  value = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].port : azurerm_redis_cache.this[0].port
}

output "primary_access_key" {
  value     = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : azurerm_redis_cache.this[0].primary_access_key
  sensitive = true
}

output "id" {
 value = azurerm_redis_cache.this[0].id
}