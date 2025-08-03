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
  metadata { name = "dev" }
}


########################################
# Grafana
########################################

module "grafana" {
  source                    = "./modules/grafana"
  namespace                 = var.grafana_namespace
  admin_user                = var.grafana_admin_user
  admin_password            = var.grafana_admin_password
  service_type              = "LoadBalancer"
  service_port              = 80
  sidecar_dashboards        = true
  persistence_enabled       = true
  persistence_size          = "5Gi"
  persistence_storage_class = var.grafana_storage_class
  persistence_access_modes  = ["ReadWriteOnce"]
  persistence_finalizers    = ["retain"]
  grafana_chart_version     = "7.3.8"
  location                  = var.location
  resource_group_name       = azurerm_resource_group.main.name
  domain_name_label         = "grafana-demo-${var.resource_group_name}"
  mc_resource_group         = var.mc_resource_group
}

module "prometheus_stack" {
  source    = "./modules/prometheus"
  namespace = "monitoring"
}

resource "kubernetes_config_map" "grafana_datasource" {
  metadata {
    name      = "grafana-datasources"
    namespace = module.grafana.namespace
    labels = {
      grafana_datasource = "1"
    }
  }

  data = {
    "datasources.yaml" = file("${path.module}/datasources.yaml")
  }
}

resource "kubernetes_config_map" "grafana_dashboard_cluster" {
  metadata {
    name      = "dashboard-cluster"
    namespace = module.grafana.namespace
    labels = {
      grafana_dashboard = "1"
    }
  }

  data = {
    "cluster-overview.json" = file("${path.module}/dashboards/cluster-overview.json")
  }
}

resource "kubernetes_config_map" "grafana_dashboard_deployments" {
  metadata {
    name      = "dashboard-deployments"
    namespace = module.grafana.namespace
    labels = {
      grafana_dashboard = "1"
    }
  }

  data = {
    "deployments.json" = file("${path.module}/dashboards/deployments.json")
  }
}

resource "kubernetes_config_map" "grafana_dashboard_nodes" {
  metadata {
    name      = "dashboard-nodes"
    namespace = module.grafana.namespace
    labels = {
      grafana_dashboard = "1"
    }
  }

  data = {
    "node-exporter.json" = file("${path.module}/dashboards/node-exporter.json")
  }
}

resource "kubernetes_config_map" "grafana_dashboard_k8s" {
  metadata {
    name      = "dashboard-k8s"
    namespace = module.grafana.namespace
    labels = {
      grafana_dashboard = "1"
    }
  }

  data = {
    "K8S-dashboard.json" = file("${path.module}/dashboards/K8S-dashboard.json")
  }
}