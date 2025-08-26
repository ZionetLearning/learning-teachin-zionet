output "servicebus_diag_id" {
  value = azurerm_monitor_diagnostic_setting.servicebus.id
}

output "postgres_diag_id" {
  value = azurerm_monitor_diagnostic_setting.postgres.id
}

output "signalr_diag_id" {
  value = azurerm_monitor_diagnostic_setting.signalr.id
}

output "redis_diag_id" {
  value = azurerm_monitor_diagnostic_setting.redis.id
}

# output "frontend_diag_id" {
#   value = azurerm_monitor_diagnostic_setting.frontend.id
# }