########################################
# 1. Azure infra: RG, AKS (conditional), Service Bus, Postgres and SignalR, Redis
########################################
resource "azurerm_resource_group" "main" {
  name     = "${var.environment_name}-${var.resource_group_name}"
  location = var.location

  tags = {
    Environment = var.environment_name
    ManagedBy   = "terraform"
  }
}

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
  prefix              = var.environment_name
  enable_spot_nodes   = var.environment_name != "prod"  # Disable spot nodes for production
  depends_on          = [azurerm_resource_group.main]
}

# Local values to determine which cluster to use
locals {
  aks_cluster_name     = var.use_shared_aks ? data.azurerm_kubernetes_cluster.shared[0].name : module.aks[0].cluster_name
  aks_resource_group   = var.use_shared_aks ? var.shared_resource_group : azurerm_resource_group.main.name
  kubernetes_namespace = var.kubernetes_namespace != "" ? var.kubernetes_namespace : var.environment_name
  aks_kube_config      = var.use_shared_aks ? data.azurerm_kubernetes_cluster.shared[0].kube_config[0] : module.aks[0].kube_config
  
  # Secure credential management - use GitHub Actions environment variables
  admin_username = var.admin_username
  admin_password = var.admin_password
}

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
# 3. Environment-specific namespace and resources for the workloads
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

module "servicebus" {
  source              = "./modules/servicebus"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  namespace_name      = "${var.environment_name}-${var.servicebus_namespace}"
  queue_names         = var.queue_names
  session_enabled_queues = var.session_enabled_queues
  depends_on          = [azurerm_resource_group.main]
}

#--------------------PostgreSQL-----------------------
# Logic: Respect the use_shared_postgres variable from tfvars
locals {
  # Use the variable from tfvars to determine if using shared postgres
  use_shared_postgres = var.use_shared_postgres
  postgres_server_rg  = var.use_shared_postgres ? "dev-zionet-learning-2025" : azurerm_resource_group.main.name
}

# Reference to shared PostgreSQL server (for non-dev environments)
data "azurerm_postgresql_flexible_server" "shared" {
  count               = local.use_shared_postgres ? 1 : 0
  name                = "dev-${var.database_server_name}"
  resource_group_name = local.postgres_server_rg
}

# Create new PostgreSQL server only for dev environment
module "database" {
  count  = local.use_shared_postgres ? 0 : 1
  source = "./modules/postgresql"

  server_name         = "${var.environment_name}-${var.database_server_name}"
  location            = var.db_location
  resource_group_name = azurerm_resource_group.main.name

  admin_username = var.admin_username
  admin_password = var.admin_password

  delegated_subnet_id = var.delegated_subnet_id

  environment_name = var.environment_name
  database_name    = "${var.database_name}-${var.environment_name}"

  existing_server_id  = null
  use_shared_postgres = false

  depends_on = [azurerm_resource_group.main]
}

# Create database on shared server for non-dev environments
resource "azurerm_postgresql_flexible_server_database" "shared_database" {
  count     = local.use_shared_postgres ? 1 : 0
  name      = "${var.database_name}-${var.environment_name}"
  server_id = data.azurerm_postgresql_flexible_server.shared[0].id
  charset   = "UTF8"
  collation = "en_US.utf8"

  depends_on = [data.azurerm_postgresql_flexible_server.shared]
}

# Langfuse database on the same PostgreSQL server (dev and prod environments)
resource "azurerm_postgresql_flexible_server_database" "langfuse" {
  count     = (var.enable_langfuse && (var.environment_name == "dev" || var.environment_name == "prod")) ? 1 : 0
  name      = "langfuse-${var.environment_name}"
  server_id = local.use_shared_postgres ? data.azurerm_postgresql_flexible_server.shared[0].id : module.database[0].id
  charset   = "UTF8"
  collation = "en_US.utf8"

  depends_on = [
    module.database,
    data.azurerm_postgresql_flexible_server.shared
  ]
}

module "signalr" {
  source              = "./modules/signalr"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  signalr_name        = "${var.signalr_name}-${var.environment_name}"
}

# ------------- Shared Redis -----------------------
data "azurerm_redis_cache" "shared" {
  count               = var.use_shared_redis ? 1 : 0
  name                = var.environment_name == "prod" ? "redis-teachin-prod" : var.redis_name
  resource_group_name = var.shared_resource_group
}

# Create new Redis only if not using shared
module "redis" {
  count                = var.use_shared_redis ? 0 : 1
  source               = "./modules/redis"
  name                 = var.environment_name == "prod" ? "redis-teachin-prod" : var.redis_name
  location             = azurerm_resource_group.main.location
  resource_group_name  = azurerm_resource_group.main.name
  shared_redis_name    = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].name : null
}

# Use shared Redis outputs if enabled, otherwise use module outputs
locals {
  redis_hostname = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].hostname : module.redis[0].hostname
  redis_port     = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].port : module.redis[0].port
  redis_key      = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : module.redis[0].primary_access_key
}

# ------------- Storage Account for Avatars (Optimized for Cost) -----------------------
module "storage" {
  source              = "./modules/storage"
  environment_name    = var.environment_name
}

# Monitoring - Diagnostic Settings for resources to Log Analytics
# Log Analytics Workspace - only create in dev environment
module "log_analytics" {
  source              = "./modules/log_analytics"
  environment_name    = var.environment_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
}

# Local value to determine which workspace to use (only available in dev)
locals {
  log_analytics_workspace_id = module.log_analytics.log_analytics_workspace_id
}

module "monitoring" {
  count  = var.environment_name == "dev" || var.environment_name == "prod" ? 1 : 0
  source = "./modules/monitoring"

  log_analytics_workspace_id  = local.log_analytics_workspace_id
  servicebus_namespace_id     = module.servicebus.namespace_id
  postgres_server_id          = local.use_shared_postgres ? data.azurerm_postgresql_flexible_server.shared[0].id : module.database[0].id
  signalr_id                  = module.signalr.id
  redis_id                    = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].id : module.redis[0].id
  frontend_static_web_app_id  = length(var.frontend_apps) > 0 ? [for f in module.frontend : f.static_web_app_id] : []

  frontend_application_insights_ids = length(var.frontend_apps) > 0 ? [for f in module.frontend : f.application_insights_id] : []

    depends_on = [
    module.log_analytics,
    module.servicebus,
    module.database,
    module.signalr,
    module.redis,
    module.frontend
  ]
}

module "frontend" {
  for_each = toset(var.frontend_apps)
  
  source              = "./modules/frontend"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  static_web_app_name = "${var.static_web_app_name}-${each.key}-${var.environment_name}"
  log_analytics_workspace_id = local.log_analytics_workspace_id
  
  tags = {
    Environment = var.environment_name
    Project     = "Frontend"
  }
  
  depends_on = [azurerm_resource_group.main]
}

# Reference the shared Key Vault instead of creating new ones
data "azurerm_key_vault" "shared" {
  name                = var.key_vault_name
  resource_group_name = var.key_vault_rg
}

# PostgreSQL admin credentials will come from GitHub Actions environment variables
# No need for Key Vault data sources - credentials passed as TF_VAR_* environment variables

module "clustersecretstore" {
  count       = var.environment_name == "dev" ? 1 : 0
  source     = "./modules/clustersecretstore"
  identity_id = var.identity_id
  tenant_id   = var.tenant_id
}