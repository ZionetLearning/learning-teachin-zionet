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
