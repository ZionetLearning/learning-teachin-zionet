resource "azurerm_resource_group" "main" {
  name     = "${var.environment_name}-${var.resource_group_name}"
  location = var.location

  tags = {
    Environment = var.environment_name
    ManagedBy   = "terraform"
  }
}

#########################################
# Network Infrastructure
#########################################


# only build/resolve network for dev/prod
locals {
  enable_network = contains(["dev", "prod"], var.environment_name)
}

# CREATE new network (dev/prod) when not reusing
module "network" {
  count  = local.enable_network && !var.use_existing_network ? 1 : 0
  source = "./modules/network"

  vnet_name           = "${var.environment_name}-${var.vnet_name}"
  address_space       = var.vnet_address_space
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name

  # AKS subnet
  aks_subnet_name   = "${var.environment_name}-${var.aks_subnet_name}"
  aks_subnet_prefix = var.aks_subnet_prefix

  # DB VNet + subnet
  db_vnet_name          = "${var.environment_name}-${var.db_vnet_name}"
  db_vnet_address_space = var.db_vnet_address_space
  db_vnet_location      = var.db_location
  db_subnet_name        = "${var.environment_name}-${var.db_subnet_name}"
  db_subnet_prefix      = var.db_subnet_prefix

  depends_on = [azurerm_resource_group.main]
}

# REUSE existing network (dev/prod) when requested
data "azurerm_virtual_network" "existing_main" {
  count               = local.enable_network && var.use_existing_network ? 1 : 0
  name                = var.existing_vnet_name
  resource_group_name = var.existing_network_rg
}

data "azurerm_subnet" "existing_aks" {
  count                = local.enable_network && var.use_existing_network ? 1 : 0
  name                 = var.existing_aks_subnet_name
  virtual_network_name = data.azurerm_virtual_network.existing_main[0].name
  resource_group_name  = var.existing_network_rg
}

data "azurerm_virtual_network" "existing_db" {
  count               = local.enable_network && var.use_existing_network ? 1 : 0
  name                = var.existing_db_vnet_name
  resource_group_name = var.existing_network_rg
}

data "azurerm_subnet" "existing_db_subnet" {
  count                = local.enable_network && var.use_existing_network ? 1 : 0
  name                 = var.existing_db_subnet_name
  virtual_network_name = data.azurerm_virtual_network.existing_db[0].name
  resource_group_name  = var.existing_network_rg
}

# Canonical IDs used by all consumers (AKS / Postgres etc.)
locals {
  vnet_id = local.enable_network ? (
    var.use_existing_network
    ? data.azurerm_virtual_network.existing_main[0].id
    : module.network[0].vnet_id
  ) : null

  aks_subnet_id = local.enable_network ? (
    var.use_existing_network
    ? data.azurerm_subnet.existing_aks[0].id
    : module.network[0].aks_subnet_id
  ) : null

  database_vnet_id = local.enable_network ? (
    var.use_existing_network
    ? data.azurerm_virtual_network.existing_db[0].id
    : module.network[0].database_vnet_id
  ) : null

  database_subnet_id = local.enable_network ? (
    var.use_existing_network
    ? data.azurerm_subnet.existing_db_subnet[0].id
    : module.network[0].database_subnet_id
  ) : null
}


########################################
# 1. Azure infra: RG, AKS (conditional), Service Bus, Postgres and SignalR, Redis
########################################


# Data source to reference existing shared AKS cluster
data "azurerm_kubernetes_cluster" "shared" {
  count               = var.use_shared_aks ? 1 : 0
  name                = var.shared_aks_cluster_name
  resource_group_name = var.shared_resource_group
}

# Create new AKS cluster only if not using shared
module "aks" {
  count               = var.use_shared_aks ? 0 : 1
  source              = "./modules/aks"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  cluster_name        = var.aks_cluster_name
  vm_size             = var.vm_size

  # Connect AKS to the dedicated AKS subnet
  aks_subnet_id = local.aks_subnet_id

  depends_on = [azurerm_resource_group.main, module.network]
}

# Local values to determine which cluster to use
locals {
  aks_cluster_name     = var.use_shared_aks ? data.azurerm_kubernetes_cluster.shared[0].name : module.aks[0].cluster_name
  aks_resource_group   = var.use_shared_aks ? var.shared_resource_group : azurerm_resource_group.main.name
  kubernetes_namespace = var.kubernetes_namespace != "" ? var.kubernetes_namespace : var.environment_name
  aks_kube_config      = var.use_shared_aks ? data.azurerm_kubernetes_cluster.shared[0].kube_config[0] : module.aks[0].kube_config
}

module "servicebus" {
  source                 = "./modules/servicebus"
  resource_group_name    = azurerm_resource_group.main.name
  location               = var.location
  namespace_name         = "${var.environment_name}-${var.servicebus_namespace}"
  sku                    = var.servicebus_sku
  queue_names            = var.queue_names
  session_enabled_queues = var.session_enabled_queues
  depends_on             = [azurerm_resource_group.main]
}
#--------------------PostgreSQL-----------------------
## Shared PostgreSQL server (created only once, in main RG)
data "azurerm_postgresql_flexible_server" "shared" {
  count               = var.use_shared_postgres ? 1 : 0
  name                = var.database_server_name
  resource_group_name = var.shared_resource_group
}

# Create new PostgreSQL server and database only if not using shared
module "database" {
  count  = var.use_shared_postgres ? 0 : 1
  source = "./modules/postgresql"

  server_name         = "prod-pg-zionet-learning"
  location            = var.db_location
  resource_group_name = var.use_shared_postgres ? var.shared_resource_group : azurerm_resource_group.main.name

  admin_username = var.admin_username
  admin_password = var.admin_password

  db_version = var.db_version
  sku_name   = var.sku_name
  storage_mb = var.storage_mb

  password_auth_enabled         = var.password_auth_enabled
  active_directory_auth_enabled = var.active_directory_auth_enabled

  backup_retention_days        = var.backup_retention_days
  geo_redundant_backup_enabled = var.geo_redundant_backup_enabled

  database_name = "${var.database_name}-${var.environment_name}"

  # Private networking configuration
  db_subnet_id           = local.database_subnet_id
  virtual_network_id     = local.database_vnet_id
  aks_virtual_network_id = local.vnet_id

  use_shared_postgres = var.use_shared_postgres
  existing_server_id  = var.use_shared_postgres ? data.azurerm_postgresql_flexible_server.shared[0].id : null

  depends_on = [azurerm_resource_group.main, module.network]
}

module "signalr" {
  source              = "./modules/signalr"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  signalr_name        = "${var.signalr_name}-${var.environment_name}"
  sku_name            = var.signalr_sku_name
  sku_capacity        = var.signalr_sku_capacity
}

# ------------- Shared Redis -----------------------
data "azurerm_redis_cache" "shared" {
  count               = var.use_shared_redis ? 1 : 0
  name                = var.redis_name
  resource_group_name = var.shared_resource_group
}

# Create new Redis only if not using shared
module "redis" {
  count               = var.use_shared_redis ? 0 : 1
  source              = "./modules/redis"
  name                = var.redis_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku_name            = "Basic"
  family              = "C"
  capacity            = 0
  shard_count         = 0
  use_shared_redis    = var.use_shared_redis

  # Note: Redis uses public endpoint with firewall rules (Basic SKU)
  # Private endpoint integration can be added later as separate module

  depends_on = [azurerm_resource_group.main]
}

# Use shared Redis outputs if enabled, otherwise use module outputs
locals {
  redis_hostname = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].hostname : module.redis[0].hostname
  redis_port     = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].port : module.redis[0].port
  redis_key      = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : module.redis[0].primary_access_key
}

# # Monitoring - Diagnostic Settings for resources to Log Analytics
# # Log Analytics Workspace - only create in dev environment
# resource "azurerm_log_analytics_workspace" "main" {
#   count               = var.environment_name == "dev" ? 1 : 0
#   name                = "${var.environment_name}-laworkspace"
#   location            = azurerm_resource_group.main.location
#   resource_group_name = azurerm_resource_group.main.name
#   sku                 = "PerGB2018"
#   retention_in_days   = 30
#   daily_quota_gb      = 1

#   tags = {
#     Environment = var.environment_name
#   }
# }

# # Local value to determine which workspace to use (only available in dev)
# locals {
#   log_analytics_workspace_id = var.environment_name == "dev" ? azurerm_log_analytics_workspace.main[0].id : null
# }

# # module "monitoring" {
# #   count  = var.environment_name == "dev" ? 1 : 0
# #   source = "./modules/monitoring"

#   log_analytics_workspace_id  = local.log_analytics_workspace_id
#   servicebus_namespace_id     = module.servicebus.namespace_id
#   postgres_server_id          = module.database[0].id
#   signalr_id                  = module.signalr.id
#   redis_id                    = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].id : module.redis[0].id
#   frontend_static_web_app_id  = length(var.frontend_apps) > 0 ? [for f in module.frontend : f.static_web_app_id] : []

#   frontend_application_insights_ids = length(var.frontend_apps) > 0 ? [for f in module.frontend : f.application_insights_id] : []

# #   depends_on = [
# #     azurerm_log_analytics_workspace.main,
# #     module.servicebus,
# #     module.database,
# #     module.signalr,
# #     module.redis,
# #     module.frontend
# #   ]
# # }

########################################
# 2. AKS kube-config for providers
########################################

resource "null_resource" "aks_ready" {
  # This resource completes when the AKS cluster (shared or new) is ready
  count = 1

  # Use triggers to ensure this runs after the appropriate AKS resource is ready
  triggers = {
    cluster_name = local.aks_cluster_name
    cluster_rg   = local.aks_resource_group
  }

  # Implicit dependency on the AKS module when not using shared AKS
  depends_on = [
    module.aks,
    data.azurerm_kubernetes_cluster.shared
  ]
}

# Data source for the cluster to be used (either shared or new)
data "azurerm_kubernetes_cluster" "main" {
  name                = local.aks_cluster_name
  resource_group_name = local.aks_resource_group

  depends_on = [null_resource.aks_ready]
}

########################################
# 4. Environment-specific namespace and resources for the workloads
########################################

# Create namespace for the environment
resource "kubernetes_namespace" "environment" {
  metadata {
    name = local.kubernetes_namespace
    labels = {
      environment = var.environment_name
      managed-by  = "terraform"
      created-by  = "terraform"
      purpose     = "application-deployment"
    }

    annotations = {
      "kubernetes.io/managed-by" = "terraform"
      "terraform.io/environment" = var.environment_name
    }
  }

  depends_on = [data.azurerm_kubernetes_cluster.main]
}

# Create service account for the environment
resource "kubernetes_service_account" "environment" {
  metadata {
    name      = "${var.environment_name}-serviceaccount"
    namespace = kubernetes_namespace.environment.metadata[0].name

    labels = {
      environment = var.environment_name
      managed-by  = "terraform"
    }

    annotations = {
      "kubernetes.io/managed-by" = "terraform"
    }
  }

  depends_on = [kubernetes_namespace.environment]
}

# module "frontend" {
#   for_each = toset(var.frontend_apps)

#   source                          = "./modules/frontend"
#   resource_group_name             = azurerm_resource_group.main.name
#   location                        = azurerm_resource_group.main.location
#   static_web_app_name             = "${var.static_web_app_name}-${each.key}-${var.environment_name}"
#   sku_tier                        = var.frontend_sku_tier
#   sku_size                        = var.frontend_sku_size
#   appinsights_retention_days      = var.frontend_appinsights_retention_days
#   appinsights_sampling_percentage = var.frontend_appinsights_sampling_percentage

#   log_analytics_workspace_id = local.log_analytics_workspace_id

#   tags = {
#     Environment = var.environment_name
#     Project     = "Frontend"
#   }

#   depends_on = [azurerm_resource_group.main]
# }


# Reference the shared Key Vault instead of creating new ones
data "azurerm_key_vault" "shared" {
  name                = "teachin-seo-kv"
  resource_group_name = "dev-zionet-learning-2025"
}

module "clustersecretstore" {
  count       = var.environment_name == "dev" || var.environment_name == "prod" ? 1 : 0
  source      = "./modules/clustersecretstore"
  identity_id = "d1ec4dfd-cefc-4eb9-a7a3-71eeddbaa744" # AKS cluster prod agent pool managed identity clientId
  tenant_id   = "a814ee32-f813-4a36-9686-1b9268183e27"
}


