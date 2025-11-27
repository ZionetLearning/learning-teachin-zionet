########################
# Service Bus secret
########################
resource "azurerm_key_vault_secret" "azure_service_bus" {
  name         = "${var.environment_name}-azure-service-bus-secret"
  value        = module.servicebus.connection_string
  key_vault_id = data.azurerm_key_vault.shared.id
}

########################
# PostgreSQL secret
########################
resource "azurerm_key_vault_secret" "postgres_connection" {
  name  = "${var.environment_name}-postgres-connection"
  value = (
    local.use_shared_postgres
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
# Communication Service secret
########################
resource "azurerm_key_vault_secret" "communication_service_connection" {
  name         = "${var.environment_name}-comm-svc-connection"
  value        = var.communication_service_connection_string
  key_vault_id = data.azurerm_key_vault.shared.id
}

########################
# Storage Account secret for Avatars
########################
resource "azurerm_key_vault_secret" "avatars_storage_connection" {
  name         = "${var.environment_name}-avatars-storage-connection"
  value        = module.storage.connection_string
  key_vault_id = data.azurerm_key_vault.shared.id
}

########################
# Engine Tavily API Key secret - Environment-specific for dev and prod
########################
resource "azurerm_key_vault_secret" "engine_tavily_apikey" {
  count        = var.environment_name == "dev" || var.environment_name == "prod" ? 1 : 0
  name         = "engine-tavily-apikey"
  value        = var.tavily_api_key
  key_vault_id = data.azurerm_key_vault.shared.id
}

########################
# Application Secrets for Services
########################
# Azure OpenAI API Key for Engine
resource "azurerm_key_vault_secret" "engine_azureopenai_apikey" {
  name         = "engine-azureopenai-apikey"
  value        = var.azure_openai_api_key != null ? var.azure_openai_api_key : "placeholder-openai-key-not-configured"
  key_vault_id = data.azurerm_key_vault.shared.id
}

# Azure Speech Service Key for Engine
resource "azurerm_key_vault_secret" "engine_azurespeech_subscriptionkey" {
  name         = "engine-azurespeech-subscriptionkey"
  value        = var.azure_speech_key != null ? var.azure_speech_key : "placeholder-speech-key-not-configured"
  key_vault_id = data.azurerm_key_vault.shared.id
}

# Azure Speech Service Key for Accessor
resource "azurerm_key_vault_secret" "accessor_speech_key" {
  name         = "accessor-speech-key"
  value        = var.azure_speech_key != null ? var.azure_speech_key : "placeholder-speech-key-not-configured"
  key_vault_id = data.azurerm_key_vault.shared.id
}

# JWT Secrets for Manager
resource "azurerm_key_vault_secret" "manager_jwt_secret" {
  name         = "manager-jwt-secret"
  value        = var.jwt_secret != null ? var.jwt_secret : random_password.jwt_secret.result
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "manager_jwt_issuer" {
  name         = "manager-jwt-issuer"
  value        = var.jwt_issuer
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "manager_jwt_audience" {
  name         = "manager-jwt-audience"
  value        = var.jwt_audience
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "manager_jwt_refreshtokenhashkey" {
  name         = "manager-jwt-refreshtokenhashkey"
  value        = var.jwt_refresh_token_hash_key != null ? var.jwt_refresh_token_hash_key : random_password.jwt_refresh_hash.result
  key_vault_id = data.azurerm_key_vault.shared.id
}

# Generate secure random passwords for JWT secrets if not provided
resource "random_password" "jwt_secret" {
  length  = 64
  special = true
}

resource "random_password" "jwt_refresh_hash" {
  length  = 32
  special = true
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
  value        = "redis-password-change-in-production"  # Keeping original for now to minimize changes
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
  value        = "postgresql://${var.admin_username}:${var.admin_password}@${var.environment_name}-${var.database_server_name}.postgres.database.azure.com:5432/langfuse-${var.environment_name}?schema=public&sslmode=require"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_direct_url" {
  name         = "${var.environment_name}-langfuse-direct-url"
  value        = "postgresql://${var.admin_username}:${var.admin_password}@${var.environment_name}-${var.database_server_name}.postgres.database.azure.com:5432/langfuse-${var.environment_name}?schema=public&sslmode=require"
  key_vault_id = data.azurerm_key_vault.shared.id
}

########################
# Langfuse API Keys - Environment-specific for dev and prod
########################
resource "azurerm_key_vault_secret" "langfuse_baseurl" {
  count        = var.environment_name == "dev" || var.environment_name == "prod" ? 1 : 0
  name         = "langfuse-baseurl"
  value        = var.environment_name == "prod" ? "https://teachin-prod.westeurope.cloudapp.azure.com/langfuse" : "https://teachin.westeurope.cloudapp.azure.com/langfuse"
  key_vault_id = data.azurerm_key_vault.shared.id
}



resource "azurerm_key_vault_secret" "langfuse_public_key" {
  count        = var.environment_name == "dev" || var.environment_name == "prod" ? 1 : 0
  name         = "langfuse-public-key"
  value        = "pk-lf-78a4be40-1031-43d6-b2a0-4b1cf15f8ff6"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "langfuse_secret_key" {
  count        = var.environment_name == "dev" || var.environment_name == "prod" ? 1 : 0
  name         = "langfuse-secret-key"
  value        = "sk-lf-7e889621-246f-4bdb-8954-d298ef5d67a1"
  key_vault_id = data.azurerm_key_vault.shared.id
}