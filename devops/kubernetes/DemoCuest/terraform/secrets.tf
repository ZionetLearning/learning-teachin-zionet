########################
# Service Bus secret
########################
resource "kubernetes_secret" "azure_service_bus" {
  metadata {
    name      = "azure-service-bus-secret"                 # <── used by every Dapr queue binding
    namespace = kubernetes_namespace.model.metadata[0].name
  }

  data = {
    AzureServiceBusConnectionString = module.servicebus.connection_string
  }
}

########################
# Cosmos DB secret
########################
# resource "kubernetes_secret" "cosmosdb_connection" {
#   metadata {
#     name      = "cosmosdb-connection"                      # <── used by todoaccessor YAML
#     namespace = kubernetes_namespace.model.metadata[0].name
#   }

#   data = {
#     CosmosDbConnectionString = module.cosmosdb.connection_string
#   }
# }

########################
# SignalR secret
########################
resource "kubernetes_secret" "signalr_connection" {
  metadata {
    name      = "signalr-connection"
    namespace = kubernetes_namespace.model.metadata[0].name
  }
  data = {
    SignalRConnectionString = module.signalr.primary_connection_string
  }
}
