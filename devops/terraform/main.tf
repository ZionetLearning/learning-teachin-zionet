########################################
# 1. Azure infra: RG, AKS, Service Bus, Postgres and SignalR, Redis
########################################
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

module "aks" {
  source              = "./modules/aks"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  cluster_name        = var.aks_cluster_name
  vm_size             = var.vm_size
  # mc_resource_group_name = "MC_${var.resource_group_name}_${var.aks_cluster_name}_${var.location}"
  depends_on = [azurerm_resource_group.main]
}

module "servicebus" {
  source              = "./modules/servicebus"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  namespace_name      = var.servicebus_namespace
  sku                 = var.servicebus_sku
  queue_names         = var.queue_names
  
  depends_on = [azurerm_resource_group.main]
}

module "database" {
  source              = "./modules/postgresql"

  server_name         = var.database_server_name
  location            = var.db_location
  resource_group_name = azurerm_resource_group.main.name

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
  # aks_public_ip       = module.aks.public_ip_address

  depends_on = [azurerm_resource_group.main]
}



module "signalr" {
  source              = "./modules/signalr"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  signalr_name        = var.signalr_name
  sku_name            = var.signalr_sku_name
  sku_capacity        = var.signalr_sku_capacity
}

module "redis" {
  source              = "./modules/redis"
  name                = "teachin-redis-keda"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku_name            = "Basic"
  family              = "C"
  capacity            = 0
  shard_count         = 0
}

module "frontend" {
  source              = "./modules/frontend"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  static_web_app_name = var.static_web_app_name
  sku_tier            = var.frontend_sku_tier
  sku_size            = var.frontend_sku_size
  appinsights_retention_days = var.frontend_appinsights_retention_days
  appinsights_sampling_percentage = var.frontend_appinsights_sampling_percentage
  
  tags = {
    Environment = "Development"
    Project     = "Frontend"
  }
  
  depends_on = [azurerm_resource_group.main]
}

########################################
# 2. AKS kube-config for providers
########################################
data "azurerm_kubernetes_cluster" "main" {
  name                = module.aks.cluster_name
  resource_group_name = azurerm_resource_group.main.name
  depends_on          = [module.aks]
}

provider "kubernetes" {
  host                   = module.aks.kube_config["host"]
  client_certificate     = base64decode(module.aks.kube_config["client_certificate"])
  client_key             = base64decode(module.aks.kube_config["client_key"])
  cluster_ca_certificate = base64decode(module.aks.kube_config["cluster_ca_certificate"])
}

provider "helm" {
  kubernetes = {
    host                   = data.azurerm_kubernetes_cluster.main.kube_config[0].host
    client_certificate     = base64decode(data.azurerm_kubernetes_cluster.main.kube_config[0].client_certificate)
    client_key             = base64decode(data.azurerm_kubernetes_cluster.main.kube_config[0].client_key)
    cluster_ca_certificate = base64decode(data.azurerm_kubernetes_cluster.main.kube_config[0].cluster_ca_certificate)
  }
}

########################################
# 4. devops-model namespace for the workloads
########################################
resource "kubernetes_namespace" "model" {
  metadata { name = "dev" }
}