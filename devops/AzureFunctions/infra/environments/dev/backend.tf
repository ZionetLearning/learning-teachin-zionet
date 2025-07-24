terraform {
  backend "azurerm" {
    resource_group_name  = "tfstate-rg"
    storage_account_name = "teachintfstate"
    container_name       = "tfstate-azurefunction-dev"
    key                  = "azure-functions-dev.tfstate"
  }
}