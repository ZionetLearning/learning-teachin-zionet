output "postgres_fqdn" {
  value       = var.use_shared_postgres ? "" : azurerm_postgresql_flexible_server.this[0].fqdn
  description = "Fully qualified domain name of the PostgreSQL server"
}

output "postgres_admin_username" {
  value       = var.use_shared_postgres ? "" : azurerm_postgresql_flexible_server.this[0].administrator_login
  description = "Administrator username"
}

output "postgres_database_name" {
  value       = var.database_name
  description = "Database name"
}

output "postgres_connection_string" {
  value = var.use_shared_postgres ? "" : format("Host=%s;Database=%s;Username=%s;Password=%s;SslMode=Require", azurerm_postgresql_flexible_server.this[0].fqdn, var.database_name, azurerm_postgresql_flexible_server.this[0].administrator_login, var.admin_password)
  description = "Full PostgreSQL connection string"
  sensitive   = true
}

output "id" {
  value = var.use_shared_postgres ? var.existing_server_id : azurerm_postgresql_flexible_server.this[0].id
  description = "The ID of the PostgreSQL flexible server"
}

output "private_dns_zone_id" {
  value       = var.use_shared_postgres ? null : azurerm_private_dns_zone.postgres[0].id
  description = "ID of the private DNS zone for PostgreSQL"
}

output "private_dns_zone_name" {
  value       = var.use_shared_postgres ? null : azurerm_private_dns_zone.postgres[0].name
  description = "Name of the private DNS zone for PostgreSQL"
}