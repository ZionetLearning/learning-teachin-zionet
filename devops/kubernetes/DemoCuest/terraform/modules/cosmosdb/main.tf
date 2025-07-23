resource "azurerm_cosmosdb_account" "main" {
  name                = var.cosmosdb_account_name
  location            = var.location
  resource_group_name = var.resource_group_name

  offer_type          = "Standard"  # Required even for serverless
  kind                = "GlobalDocumentDB"  # SQL API

  capabilities {
    name = "EnableServerless"
  }

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }
}
