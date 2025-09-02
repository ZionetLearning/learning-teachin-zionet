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

# SignalR
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

resource "azurerm_monitor_diagnostic_setting" "application_insights" {
  count                      = length(var.frontend_application_insights_ids)
  name                       = "appinsights-diag-${count.index}"
  target_resource_id         = var.frontend_application_insights_ids[count.index]
  log_analytics_workspace_id = var.log_analytics_workspace_id

  # Only the logs we need for basic monitoring
  enabled_log {
    category = "AppRequests"
  }
  
  enabled_log {
    category = "AppPageViews" 
  }
  
  enabled_log {
    category = "AppExceptions"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}