# PostgreSQL Flexible Server with Public Endpoint
# This configuration uses public endpoints with firewall rules for security
# VNet integration can be added later as needed

resource "azurerm_postgresql_flexible_server" "this" {
  count                   = var.use_shared_postgres ? 0 : 1
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


  # # Enable public network access for simplified connectivity
  # public_network_access_enabled = true

  authentication {
    password_auth_enabled         = var.password_auth_enabled
    active_directory_auth_enabled = var.active_directory_auth_enabled
  }

  # Note: Not using VNet integration for now - keeping it simple
  # delegated_subnet_id = null
  # private_dns_zone_id = null

  # Add lifecycle rule to prevent zone changes
  lifecycle {
    ignore_changes = [
      zone,
      high_availability
    ]
  }
}

# # Create firewall rule to allow access from Azure services
# resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure_services" {
#   count            = var.use_shared_postgres ? 0 : 1
#   name             = "AllowAzureServices"
#   server_id        = azurerm_postgresql_flexible_server.this[0].id
#   start_ip_address = "0.0.0.0"
#   end_ip_address   = "0.0.0.0"
# }



resource "azurerm_postgresql_flexible_server_database" "this" {
  name      = var.database_name
  server_id = var.use_shared_postgres ? var.existing_server_id : azurerm_postgresql_flexible_server.this[0].id
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