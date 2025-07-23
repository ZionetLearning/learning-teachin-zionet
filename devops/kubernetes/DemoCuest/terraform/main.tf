########################################
# 1. Azure infra: RG, AKS, Service Bus, Cosmos
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
  sku                 = var.sku
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

module "cosmosdb" {
  source                = "./modules/cosmosdb"
  resource_group_name   = azurerm_resource_group.main.name
  location              = "North Europe" # over ride because it made error that 'full'
  cosmosdb_account_name = var.cosmosdb_account_name
  cosmosdb_sql_database_name = var.cosmosdb_sql_database_name
  cosmosdb_sql_container_name = var.cosmosdb_sql_container_name
  cosmosdb_partition_key_path = var.cosmosdb_partition_key_path
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

# ########################################
# # 5. Apply *all* YAML manifests under ./k8s
# ########################################
# module "k8s_manifests" {
#   source          = "./modules/k8s_manifests"
#   k8s_dir         = "${path.module}/../k8s"
#   docker_registry = var.docker_registry

#   namespace = kubernetes_namespace.model.metadata[0].name

#   # pass the alias exactly as the child module expects
#   providers = {
#     kubectl.inherited = kubectl.inherited
#   }

#   depends_on = [
#     kubernetes_secret.azure_service_bus,
#     kubernetes_secret.cosmosdb_connection,
#     helm_release.dapr
#   ]
# }

### how to start
### terraform init
### terraform plan -var-file="terraform.tfvars.dev"
### terraform apply -var-file="terraform.tfvars.dev"

### how to destroy
### ### terraform destroy -var-file="terraform.tfvars.dev"

### az aks get-credentials   --resource-group democuest-aks-rg-dev   --name democuest-aks-dev   --overwrite-existing
### to be able to '  kubectl get pods -n devops-model   ' 

### to get external ip '   kubectl -n devops-model get svc todomanager   '

### see logs of pods '   $ kubectl -n devops-model logs deployment/todoaccessor -f   '

### apply new/updated yaml '    kubectl apply -f ./todoaccessor-deployment.yaml -n devops-model   '

### restart pod  '   kubectl rollout restart deployment/todoaccessor -n devops-model  '