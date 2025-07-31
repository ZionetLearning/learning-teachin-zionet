output "document_endpoint" {
  description = "Cosmos DB endpoint URI"
  value       = azurerm_cosmosdb_account.main.endpoint
}

output "primary_key" {
  description = "Primary key for Cosmos DB"
  value       = azurerm_cosmosdb_account.main.primary_key
}

output "connection_string" {
  description = "Manually composed CosmosDB connection string"
  value = "AccountEndpoint=${azurerm_cosmosdb_account.main.endpoint};AccountKey=${azurerm_cosmosdb_account.main.primary_key};"
}
