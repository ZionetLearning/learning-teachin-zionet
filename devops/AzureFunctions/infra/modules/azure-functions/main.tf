resource "azurerm_linux_function_app" "this" {
  name                =  var.function_app_name
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
  }

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"     = "dotnet-isolated"
    "FUNCTIONS_EXTENSION_VERSION"  = "~4"
    "DOTNET_VERSION"              = "8.0"
    "FUNCTIONS_INPROC_NET8_ENABLED" = "1"
    "AzureWebJobsStorage"         = "DefaultEndpointsProtocol=https;AccountName=${var.storage_account_name};AccountKey=${var.storage_account_access_key};EndpointSuffix=core.windows.net"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = "DefaultEndpointsProtocol=https;AccountName=${var.storage_account_name};AccountKey=${var.storage_account_access_key};EndpointSuffix=core.windows.net"
    "WEBSITE_CONTENTSHARE"        = var.function_app_name
    "ServiceBusConnectionString"  = var.service_bus_connection_string
  }
}