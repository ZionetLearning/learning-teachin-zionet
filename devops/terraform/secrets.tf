########################
# Service Bus secret
########################
resource "azurerm_key_vault_secret" "azure_service_bus" {
  name         = "${var.environment_name}-azure-service-bus-secret"
  value        = module.servicebus.connection_string
  key_vault_id = azurerm_key_vault.main.id
}

########################
# PostgreSQL secret
########################
resource "azurerm_key_vault_secret" "postgres_connection" {
  name         = "${var.environment_name}-postgres-connection"
  value        = module.database.postgres_connection_string
  key_vault_id = azurerm_key_vault.main.id
}

########################
# SignalR secret
########################
resource "azurerm_key_vault_secret" "signalr_connection" {
  name         = "${var.environment_name}-signalr-connection"
  value        = module.signalr.primary_connection_string
  key_vault_id = azurerm_key_vault.main.id
}

########################
# Redis secrets
########################
resource "azurerm_key_vault_secret" "redis_hostport" {
  name         = "${var.environment_name}-redis-hostport"
  value        = "${module.redis.hostname}:6380"
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "redis_password" {
  name         = "${var.environment_name}-redis-password"
  value        = module.redis.primary_access_key
  key_vault_id = azurerm_key_vault.main.id
}
