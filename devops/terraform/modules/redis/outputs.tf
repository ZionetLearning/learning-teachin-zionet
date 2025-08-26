output "hostname" {
  value = azurerm_redis_cache.this.hostname
}

output "port" {
  value = azurerm_redis_cache.this.port
}

output "primary_access_key" {
  value     = azurerm_redis_cache.this.primary_access_key
  sensitive = true
}

output "id" {
  value = azurerm_redis_cache.this.id
}