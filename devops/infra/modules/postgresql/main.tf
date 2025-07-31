resource "azurerm_postgresql_flexible_server" "this" {
  name                   = var.server_name
  location               = var.location
  resource_group_name    = var.resource_group_name
  administrator_login    = var.admin_username
  administrator_password = var.admin_password

  version    = var.db_version
  sku_name   = var.sku_name
  storage_mb = var.storage_mb

  backup_retention_days        = var.backup_retention_days
  geo_redundant_backup_enabled = var.geo_redundant_backup_enabled

  # Explicitly set zone to null for Basic SKUs or remove zone entirely
  zone = null

  authentication {
    password_auth_enabled         = var.password_auth_enabled
    active_directory_auth_enabled = var.active_directory_auth_enabled
  }

  delegated_subnet_id = var.delegated_subnet_id

  # Add lifecycle rule to prevent zone changes
  lifecycle {
    ignore_changes = [
      zone,
      high_availability
    ]
  }

}

resource "azurerm_postgresql_flexible_server_database" "this" {
  name      = var.database_name
  server_id = azurerm_postgresql_flexible_server.this.id
  charset   = "UTF8"
  collation = "en_US.utf8"

  # prevent the possibility of accidental data loss
  # lifecycle {
  #    prevent_destroy = true
  # }
}

# resource "azurerm_postgresql_flexible_server_firewall_rule" "aks_integration" {
#   name                = "aks-integration"
#   server_id           = azurerm_postgresql_flexible_server.this.id
#   start_ip_address    = var.aks_public_ip
#   end_ip_address      = var.aks_public_ip
# }