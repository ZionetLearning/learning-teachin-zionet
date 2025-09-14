output "primary_connection_string" {
  description = "Primary SignalR connection string"
  value       = azurerm_signalr_service.this.primary_connection_string
}

output "primary_connection_string_base64" {
  description = "Primary SignalR connection string (base64-encoded)"
  value       = base64encode(azurerm_signalr_service.this.primary_connection_string)
}

output "id" {
  value = azurerm_signalr_service.this.id
}

output "hostname" {
  description = "SignalR service hostname"
  value       = azurerm_signalr_service.this.hostname
}