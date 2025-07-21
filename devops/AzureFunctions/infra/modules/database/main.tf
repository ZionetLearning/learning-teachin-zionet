resource "azurerm_cosmosdb_account" "this" {
  name                = var.cosmos_account_name
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = var.offer_type
  kind                = var.kind

  consistency_policy {
    consistency_level = var.consistency_policy.consistency_level
  }

  geo_location {
    location          = var.location
    failover_priority = var.failover_priority
  }

  capabilities {
    name = var.capabilities[0].name
  }

  depends_on = [azurerm_resource_group.this]
}

resource "azurerm_cosmosdb_mongo_database" "this" {
  name                = var.database_name
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.this.name
}