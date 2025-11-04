output "hostname" {
  value = azurerm_redis_cache.this[0].hostname
}

output "port" {
  value = azurerm_redis_cache.this[0].port
}

output "primary_access_key" {
  value     = azurerm_redis_cache.this[0].primary_access_key
  sensitive = true
}

output "id" {
 value = azurerm_redis_cache.this[0].id
}