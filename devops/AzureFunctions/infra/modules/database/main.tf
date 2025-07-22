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

  authentication {
    password_auth_enabled         = var.password_auth_enabled
    active_directory_auth_enabled = var.active_directory_auth_enabled
  }

  dynamic "high_availability" {
    for_each = var.high_availability_mode != "" && var.high_availability_mode != null ? [1] : []
    content {
      mode = var.high_availability_mode
    }
  }

  delegated_subnet_id = var.delegated_subnet_id
}