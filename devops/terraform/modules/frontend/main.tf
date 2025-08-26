# Frontend Static Web App and Application Insights Module

# Create the static web app
resource "azurerm_static_web_app" "frontend" {
  name                = var.static_web_app_name
  resource_group_name = var.resource_group_name
  location            = var.location
  sku_tier            = var.sku_tier
  sku_size            = var.sku_size
  
  tags = var.tags
}

# Configure custom domain if enabled (CNAME only)
resource "azurerm_static_web_app_custom_domain" "custom" {
  count               = var.custom_domain_enabled ? 1 : 0
  static_web_app_id   = azurerm_static_web_app.frontend.id
  domain_name         = var.custom_domain_name
  validation_type     = "cname-delegation"
  
  depends_on = [azurerm_static_web_app.frontend]
  
  lifecycle {
    # Prevent accidental destruction
    prevent_destroy = false
  }
  
  # Add timeouts to handle DNS validation
  timeouts {
    create = "10m"
    delete = "5m"
  }
}

# Create Application Insights for frontend monitoring
resource "azurerm_application_insights" "frontend" {
  name                = "${var.static_web_app_name}-appinsights"
  location            = var.location
  resource_group_name = var.resource_group_name
  application_type    = "web"
  
  # Use configurable settings
  retention_in_days   = var.appinsights_retention_days
  sampling_percentage = var.appinsights_sampling_percentage
  
  # Ignore workspace_id changes to avoid conflicts with existing resource
  lifecycle {
    ignore_changes = [workspace_id]
  }
  
  tags = var.tags
}
