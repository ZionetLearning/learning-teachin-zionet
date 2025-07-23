resource "azurerm_linux_function_app" "this" {
  for_each = var.function_apps
  
  name                = each.value.name
  resource_group_name = var.resource_group_name
  location            = var.location
  storage_account_name       = var.storage_account_name
  storage_account_access_key = var.storage_account_access_key
  service_plan_id            = var.app_service_plan_id
  

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
    
    cors {
      allowed_origins     = each.value.cors_allowed_origins
      support_credentials = each.value.cors_support_credentials
    }
  }

  app_settings = merge({
    "FUNCTIONS_WORKER_RUNTIME"     = "dotnet-isolated"
    "FUNCTIONS_EXTENSION_VERSION"  = "~4"
    "DOTNET_VERSION"              = "8.0"
    "FUNCTIONS_INPROC_NET8_ENABLED" = "1"
    "AzureWebJobsStorage"         = "DefaultEndpointsProtocol=https;AccountName=${var.storage_account_name};AccountKey=${var.storage_account_access_key};EndpointSuffix=core.windows.net"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = "DefaultEndpointsProtocol=https;AccountName=${var.storage_account_name};AccountKey=${var.storage_account_access_key};EndpointSuffix=core.windows.net"
    "WEBSITE_CONTENTSHARE"        = each.value.name
    "ServiceBusConnectionString"  = var.service_bus_connection_string
  }, each.value.app_settings)

  # Enable managed identity for security best practices
  identity {
    type = "SystemAssigned"
  }

  # Enable monitoring and logging
  tags = merge(var.common_tags, {
    "FunctionType" = each.value.function_type
    "Environment"  = each.value.environment
  })
}