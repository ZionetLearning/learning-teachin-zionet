# ------------- Log Analytics Workspace -----------------------
resource "azurerm_log_analytics_workspace" "main" {
  count               = var.environment_name == "dev" || var.environment_name == "prod" ? 1 : 0
  name                = "${var.environment_name}-laworkspace"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku
  retention_in_days   = var.retention_in_days
  daily_quota_gb      = var.daily_quota_gb

  tags = {
    Environment = var.environment_name
  }
}