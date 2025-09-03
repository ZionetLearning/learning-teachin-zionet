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
  value = azurerm_postgresql_flexible_server.this[0].id
  description = "The ID of the PostgreSQL flexible server"
}