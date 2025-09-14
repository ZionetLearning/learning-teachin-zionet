########################
# Service Bus now uses Managed Identity (no connection string secret needed)
########################

########################
# PostgreSQL secret
########################
resource "azurerm_key_vault_secret" "postgres_connection" {
  name  = "${var.environment_name}-postgres-connection"
  value = (
    var.use_shared_postgres
    ? format(
        "Host=%s;Database=%s;Username=%s;Password=%s;SslMode=Require",
        data.azurerm_postgresql_flexible_server.shared[0].fqdn,
        "${var.database_name}-${var.environment_name}",
        var.admin_username,
        var.admin_password
      )
    : module.database[0].postgres_connection_string
  )
  key_vault_id = data.azurerm_key_vault.shared.id
}

########################
# SignalR now uses Managed Identity (no connection string secret needed)
########################

########################
# Redis secret
########################
resource "azurerm_key_vault_secret" "redis_hostport" {
  name         = "${var.environment_name}-redis-hostport"
  value        = var.use_shared_redis ? "${data.azurerm_redis_cache.shared[0].hostname}:6380" : "${module.redis[0].hostname}:6380"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "redis_password" {
  name         = "${var.environment_name}-redis-password"
  value        = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : module.redis[0].primary_access_key
  key_vault_id = data.azurerm_key_vault.shared.id
}
