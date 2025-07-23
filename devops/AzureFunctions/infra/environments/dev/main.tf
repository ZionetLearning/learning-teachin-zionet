# Configure the Azure Provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {}
}

# Create the Resource Group
module "resource_group" {
  source = "../../modules/resource-group"
  name     = var.resource_group_name
  location = var.location
}

# Create the Service Bus Namespace and Queues
module "service_bus" {
  source = "../../modules/service-bus"
  
  namespace_name      = var.servicebus_namespace_name
  resource_group_name = module.resource_group.name
  location           = module.resource_group.location
  
  queues = {
    "myqueue" = {}  
  }
}

# Create the Storage Account
module "storage_account" {
  source = "../../modules/storage-account" 
  storage_account_name =  var.storage_account_name
  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  account_tier        = var.storage_account_tier
  account_replication_type = var.storage_account_replication_type
}


# Create the App Service Plan
module "app_service_plan" {
  source = "../../modules/service-plan"
  app_service_plan_name = "asp-${var.resource_group_name}"
  location              = module.resource_group.location
  resource_group_name   = module.resource_group.name
  sku = var.app_service_plan_sku
}

# Create multiple Function Apps
module "function_apps" {
  source = "../../modules/azure-functions"
  
  function_apps = {
    for key, config in var.function_apps_config : key => {
      name                     = "${config.name}-${var.resource_group_name}"
      cors_allowed_origins     = config.cors_allowed_origins
      cors_support_credentials = config.cors_support_credentials
      # Add database connection string to accessor function
      app_settings            = key == "accessor" ? merge(config.app_settings, {
        "ConnectionStrings__DefaultConnection" = module.database.postgres_connection_string
        "Database__Provider" = "PostgreSQL"
        "Database__Host" = module.database.postgres_fqdn
        "Database__Database" = module.database.postgres_database_name
        "Database__Username" = module.database.postgres_admin_username
        "Database__Password" = module.database.postgres_admin_password
      }) : config.app_settings
      function_type           = config.function_type
      environment             = config.environment
    }
  }
  
  location                   = module.resource_group.location
  resource_group_name        = module.resource_group.name
  app_service_plan_id        = module.app_service_plan.id
  storage_account_name       = module.storage_account.name
  storage_account_access_key = module.storage_account.primary_access_key
  service_bus_connection_string = module.service_bus.connection_string
  common_tags               = var.common_tags
}




module "database" {
  source              = "../../modules/database"

  server_name         = "pg-${var.resource_group_name}"
  location            = var.db_location
  resource_group_name = module.resource_group.name

  admin_username      = var.admin_username
  admin_password      = var.admin_password

  db_version          = var.db_version
  sku_name            = var.sku_name
  storage_mb          = var.storage_mb

  password_auth_enabled         = var.password_auth_enabled
  active_directory_auth_enabled = var.active_directory_auth_enabled

  backup_retention_days         = var.backup_retention_days
  geo_redundant_backup_enabled  = var.geo_redundant_backup_enabled

  delegated_subnet_id           = var.delegated_subnet_id

  database_name       = var.database_name
}