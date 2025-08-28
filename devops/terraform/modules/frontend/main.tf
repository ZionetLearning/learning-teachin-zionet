# Frontend Static Web App and Application Insights Module

# Create the static web app
resource "azurerm_static_web_app" "frontend" {
  name                = var.static_web_app_name
  resource_group_name = var.resource_group_name
  location            = var.location
  sku_tier            = var.sku_tier
  sku_size            = var.sku_size

   app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.frontend.connection_string
  }
  
  tags = var.tags
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
