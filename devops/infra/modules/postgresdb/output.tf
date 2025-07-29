output "postgres_fqdn" {
  value       = azurerm_postgresql_flexible_server.this.fqdn
  description = "Fully qualified domain name of the PostgreSQL server"
}

output "postgres_admin_username" {
  value       = azurerm_postgresql_flexible_server.this.administrator_login
  description = "Administrator username"
}

output "postgres_database_name" {
  value       = var.database_name
  description = "Database name"
}

output "postgres_connection_string" {
  value = "Host=${azurerm_postgresql_flexible_server.this.fqdn};Database=${var.database_name};Username=${azurerm_postgresql_flexible_server.this.administrator_login};Password=${var.admin_password};SslMode=Require"
  description = "Full PostgreSQL connection string"
  sensitive   = true
}