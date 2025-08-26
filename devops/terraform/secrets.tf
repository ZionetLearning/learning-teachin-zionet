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
  name         = "redis-hostport"
  value        = var.use_shared_redis ? "${data.azurerm_redis_cache.shared[0].hostname}:6380" : "${module.redis.hostname}:6380"
  key_vault_id = data.azurerm_key_vault.shared.id
}

resource "azurerm_key_vault_secret" "redis_password" {
  name         = "redis-password"
  value        = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : module.redis.primary_access_key
  key_vault_id = data.azurerm_key_vault.shared.id
}
resource "kubernetes_manifest" "cluster_secret_store" {
  manifest = {
    apiVersion = "external-secrets.io/v1"
    kind       = "ClusterSecretStore"
    metadata = {
      name = "azure-keyvault-backend"
    }
    spec = {
      provider = {
        azurekv = {
          authType   = "ManagedIdentity"
          vaultUrl   = "https://teachin-seo-kv.vault.azure.net/"
          identityId = "0997f44d-fadf-4be8-8dc6-202f7302f680"
          tenantId   = "a814ee32-f813-4a36-9686-1b9268183e27"
        }
      }
    }
  }
}
