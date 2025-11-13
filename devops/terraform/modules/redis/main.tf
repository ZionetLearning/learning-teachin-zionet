# Create new Redis only if not using shared
resource "azurerm_redis_cache" "this" {
  count               = var.use_shared_redis ? 0 : 1
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  capacity            = var.capacity        # 0 = C0, 1 = C1, 2 = C2, ...
  family              = var.family          # "C" = Basic/Standard, "P" = Premium
  sku_name            = var.sku_name        # "Basic", "Standard", "Premium"

  minimum_tls_version = "1.2"
  shard_count         = var.shard_count

  redis_configuration {
    maxmemory_policy = "allkeys-lru"
  }
}


resource "azurerm_redis_firewall_rule" "allow_aks" {
  count = var.allowed_subnet != null && !var.use_shared_redis ? 1 : 0
  name                = "allow_aks"
  redis_cache_name    = var.shared_redis_name != null ? var.shared_redis_name : azurerm_redis_cache.this[0].name
  resource_group_name = var.resource_group_name
  start_ip            = var.allowed_subnet
  end_ip              = var.allowed_subnet
}
