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
# SignalR secret
########################
resource "azurerm_key_vault_secret" "signalr_connection" {
  name         = "${var.environment_name}-signalr-connection"
  value        = module.signalr.primary_connection_string
  key_vault_id = data.azurerm_key_vault.shared.id
}

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


########################
# Langfuse secrets (always create, controlled by Helm values)
########################
resource "azurerm_key_vault_secret" "langfuse_db_username" {
  count        = 1
  name         = "${var.environment_name}-langfuse-db-username"
  value        = var.admin_username
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_db_password" {
  count        = 1
  name         = "${var.environment_name}-langfuse-db-password"
  value        = var.admin_password
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_nextauth_secret" {
  count        = 1
  name         = "${var.environment_name}-langfuse-nextauth-secret"
  value        = "your-nextauth-secret-change-this-in-production-32chars"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_salt" {
  count        = 1
  name         = "${var.environment_name}-langfuse-salt"
  value        = "your-salt-change-this-in-production-32chars-long"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_redis_password" {
  count        = 1
  name         = "${var.environment_name}-langfuse-redis-password"
  value        = "redis-password-change-in-production"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_clickhouse_password" {
  count        = 1
  name         = "${var.environment_name}-langfuse-clickhouse-password"
  value        = "clickhouse-password-change-in-production"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_s3_user" {
  count        = 1
  name         = "${var.environment_name}-langfuse-s3-user"
  value        = "minio"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_s3_password" {
  count        = 1
  name         = "${var.environment_name}-langfuse-s3-password"
  value        = "minio-password-change-in-production"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_db_url" {
  name         = "${var.environment_name}-langfuse-db-url"
  value        = "postgresql://${var.admin_username}:${var.admin_password}@${var.database_server_name}.postgres.database.azure.com:5432/langfuse-${var.environment_name}?schema=public&sslmode=require"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_direct_url" {
  name         = "${var.environment_name}-langfuse-direct-url"
  value        = "postgresql://${var.admin_username}:${var.admin_password}@${var.database_server_name}.postgres.database.azure.com:5432/langfuse-${var.environment_name}?schema=public&sslmode=require"
  key_vault_id = data.azurerm_key_vault.shared.id
}