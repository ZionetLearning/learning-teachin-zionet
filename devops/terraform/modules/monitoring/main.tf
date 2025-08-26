############################################
# Monitoring Diagnostic Settings Module
############################################

# Service Bus
resource "azurerm_monitor_diagnostic_setting" "servicebus" {
  name                       = "servicebus-diag"
  target_resource_id         = var.servicebus_namespace_id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "OperationalLogs"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}

# PostgreSQL Flexible Server
resource "azurerm_monitor_diagnostic_setting" "postgres" {
  name                       = "postgres-diag"
  target_resource_id         = var.postgres_server_id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "PostgreSQLLogs"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}

# SignalR - Fixed: Use "AllLogs" instead of "ConnectivityLogs"
resource "azurerm_monitor_diagnostic_setting" "signalr" {
  name                       = "signalr-diag"
  target_resource_id         = var.signalr_id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "AllLogs"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}

# Redis Cache
resource "azurerm_monitor_diagnostic_setting" "redis" {
  name                       = "redis-diag"
  target_resource_id         = var.redis_id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "ConnectedClientList"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}

# NOTE: Static Web App diagnostic settings are not currently supported by Azure
# Removing this resource as it will fail with "Category 'AppLogs' is not supported"
# 
# Alternative: Monitor Static Web Apps through Application Insights integration
# which is configured separately in your frontend application code

# Uncomment this block if/when Azure adds support for Static Web App diagnostic settings
# resource "azurerm_monitor_diagnostic_setting" "frontend" {
#   name                       = "frontend-diag"
#   target_resource_id         = var.frontend_static_web_app_id
#   log_analytics_workspace_id = var.log_analytics_workspace_id
#
#   enabled_log {
#     category = "AppLogs"
#   }
#
#   enabled_metric {
#     category = "AllMetrics"
#   }
# }