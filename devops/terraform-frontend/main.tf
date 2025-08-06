# Configure the Azure Provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
  }
  
  backend "azurerm" {
    resource_group_name  = "tfstate-rg"
    storage_account_name = "teachintfstate"
    container_name       = "tfstate-frontend"
    key                  = "frontend.terraform.tfstate"
    use_azuread_auth     = true # added because of githubactions
  }
}



# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {}
}

# Create a resource group
resource "azurerm_resource_group" "frontend" {
  name     = var.resource_group_name
  location = var.location
}

# Create the static web app
resource "azurerm_static_web_app" "frontend" {
  name                = var.static_web_app_name
  resource_group_name = azurerm_resource_group.frontend.name
  location            = azurerm_resource_group.frontend.location
  sku_tier            = "Free"
  sku_size            = "Free"
}

# Create Application Insights (basic/cheapest plan)
resource "azurerm_application_insights" "frontend" {
  name                = "${var.static_web_app_name}-appinsights"
  location            = azurerm_resource_group.frontend.location
  resource_group_name = azurerm_resource_group.frontend.name
  application_type    = "web"
  
  # Use the most basic/cheapest settings
  retention_in_days   = 30  # Minimum retention
  sampling_percentage = 100 # Full sampling (can reduce to save costs)
  
  # Ignore workspace_id changes to avoid conflicts with existing resource
  lifecycle {
    ignore_changes = [workspace_id]
  }
  
  tags = {
    Environment = "Development"
    Project     = "Frontend"
  }
}
