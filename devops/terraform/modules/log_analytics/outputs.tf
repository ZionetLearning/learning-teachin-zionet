# Local value to expose workspace ID
output "log_analytics_workspace_id" {
  value       = var.environment_name == "dev" || var.environment_name == "prod" ? azurerm_log_analytics_workspace.main[0].id : null
  description = "The Log Analytics Workspace ID (only available in dev)"
}