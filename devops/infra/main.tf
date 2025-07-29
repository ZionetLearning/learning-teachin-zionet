########################################
# 1. Azure infra: RG, AKS, Service Bus, Postgres and SignalR
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
  node_count          = var.node_count
  vm_size             = var.vm_size
}

module "servicebus" {
  source              = "./modules/servicebus"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  namespace_name      = var.servicebus_namespace
  sku                 = var.servicebus_sku
  queue_names         = var.queue_names
}




module "signalr" {
  source              = "./modules/signalr"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  signalr_name        = var.signalr_name
  sku_name            = var.signalr_sku_name
  sku_capacity        = var.signalr_sku_capacity
}
########################################
# 2. AKS kube-config for providers
########################################
data "azurerm_kubernetes_cluster" "main" {
  name                = module.aks.cluster_name
  resource_group_name = azurerm_resource_group.main.name
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
# 3. Dapr control-plane
########################################
resource "helm_release" "dapr" {
  name             = "dapr"
  repository       = "https://dapr.github.io/helm-charts"
  chart            = "dapr"
  namespace        = "dapr-system"
  create_namespace = true
  version          = "1.13.0"
}

########################################
# 4. devops-model namespace for the workloads
########################################
resource "kubernetes_namespace" "model" {
  metadata { name = "devops-model" }
}
