output "cosmosdb_account_name" {
  value = azurerm_cosmosdb_account.this.name
}

output "cosmosdb_database_name" {
  value = azurerm_cosmosdb_mongo_database.this.name
}