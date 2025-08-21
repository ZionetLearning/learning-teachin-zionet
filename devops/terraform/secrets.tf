########################
# Service Bus secret
########################
resource "kubernetes_secret" "azure_service_bus" {
  metadata {
    name      = "azure-service-bus-secret"  
    namespace = kubernetes_namespace.environment.metadata[0].name   
  }

  data = {
    AzureServiceBusConnectionString = module.servicebus.connection_string
  }
}

########################
# PostgreSQL secret
########################
resource "kubernetes_secret" "postgres_connection" {
  metadata {
    name      = "postgres-connection"       
    namespace = kubernetes_namespace.environment.metadata[0].name   
  }
  data = {
    PostgreSQLConnectionString = var.use_shared_postgres ? format("Host=%s;Database=%s;Username=%s;Password=%s;SslMode=Require", data.azurerm_postgresql_flexible_server.shared[0].fqdn, "${var.database_name}-${var.environment_name}", var.admin_username, var.admin_password) : module.database[0].postgres_connection_string
  }
}

########################
# SignalR secret
########################
resource "kubernetes_secret" "signalr_connection" {
  metadata {
    name      = "signalr-connection"        
    namespace = kubernetes_namespace.environment.metadata[0].name   
  }
  data = {
    SignalRConnectionString = module.signalr.primary_connection_string
  }
}

########################
# Redis secret
########################
resource "kubernetes_secret" "redis_connection" {
  metadata {
    name      = "redis-connection"          
    namespace = kubernetes_namespace.environment.metadata[0].name   
  }
  data = {
    redis-hostport = "${module.redis.hostname}:6380"
    redis-password = module.redis.primary_access_key
  }
}
