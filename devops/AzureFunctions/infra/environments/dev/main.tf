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
    # "orders" = {
    #   max_delivery_count = 5
    # }
    # "payments" = {
    #   max_delivery_count = 1
    #   enable_partitioning = true
    # }
    # "notifications" = {}  # Use defaults
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

# Create the Function App
module "function_app" {
  source = "../../modules/azure-functions"
  
  function_app_name          = "fa-${var.resource_group_name}"
  location                   = module.resource_group.location
  resource_group_name        = module.resource_group.name
  app_service_plan_id        = module.app_service_plan.id
  storage_account_name       = module.storage_account.name
  storage_account_access_key = module.storage_account.primary_access_key
  service_bus_connection_string = module.service_bus.connection_string
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

  high_availability_mode        = var.high_availability_mode
  delegated_subnet_id           = var.delegated_subnet_id

  database_name       = var.database_name
}