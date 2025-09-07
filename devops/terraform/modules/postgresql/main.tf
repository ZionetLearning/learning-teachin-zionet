# Private DNS Zone for PostgreSQL when using VNet integration
resource "azurerm_private_dns_zone" "postgres" {
  count               = var.use_shared_postgres ? 0 : 1
  name                = "${var.server_name}.private.postgres.database.azure.com"
  resource_group_name = var.resource_group_name


}

# Link the Private DNS Zone to the VNet
resource "azurerm_private_dns_zone_virtual_network_link" "postgres" {
  count                 = var.use_shared_postgres ? 0 : 1
  name                  = "${var.server_name}-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.postgres[0].name
  virtual_network_id    = var.virtual_network_id

 
}

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

  authentication {
    password_auth_enabled         = var.password_auth_enabled
    active_directory_auth_enabled = var.active_directory_auth_enabled
  }

  # Connect PostgreSQL to the dedicated database subnet
  delegated_subnet_id = var.db_subnet_id
  
  # Set the Private DNS Zone ID for VNet integration
  private_dns_zone_id = azurerm_private_dns_zone.postgres[0].id

  # Add lifecycle rule to prevent zone changes
  lifecycle {
    ignore_changes = [
      zone,
      high_availability
    ]
  }

  # Ensure the DNS zone and link are created first
  depends_on = [
    azurerm_private_dns_zone_virtual_network_link.postgres
  ]
}

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