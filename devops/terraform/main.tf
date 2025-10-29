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
  vm_size             = var.vm_size
  depends_on          = [azurerm_resource_group.main]
}

# Local values to determine which cluster to use
locals {
  aks_cluster_name     = var.use_shared_aks ? data.azurerm_kubernetes_cluster.shared[0].name : module.aks[0].cluster_name
  aks_resource_group   = var.use_shared_aks ? var.shared_resource_group : azurerm_resource_group.main.name
  kubernetes_namespace = var.kubernetes_namespace != "" ? var.kubernetes_namespace : var.environment_name
  aks_kube_config      = var.use_shared_aks ? data.azurerm_kubernetes_cluster.shared[0].kube_config[0] : module.aks[0].kube_config
}

module "servicebus" {
  source              = "./modules/servicebus"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  namespace_name      = "${var.environment_name}-${var.servicebus_namespace}"
  sku                 = var.servicebus_sku
  queue_names         = var.queue_names
  session_enabled_queues = var.session_enabled_queues
  depends_on          = [azurerm_resource_group.main]
}
#--------------------PostgreSQL-----------------------
# Logic: dev creates PostgreSQL server, other environments use shared server from dev
locals {
  # Dev creates server, others use shared from dev environment
  use_shared_postgres = var.environment_name != "dev"
  postgres_server_rg  = var.environment_name == "dev" ? azurerm_resource_group.main.name : "dev-zionet-learning-2025"
}

# Reference to shared PostgreSQL server (for non-dev environments)
data "azurerm_postgresql_flexible_server" "shared" {
  count               = local.use_shared_postgres ? 1 : 0
  name                = var.database_server_name
  resource_group_name = local.postgres_server_rg
}

# Create new PostgreSQL server only for dev environment
module "database" {
  count  = local.use_shared_postgres ? 0 : 1
  source = "./modules/postgresql"

  server_name         = var.database_server_name
  location            = var.db_location
  resource_group_name = azurerm_resource_group.main.name

  admin_username = var.admin_username
  admin_password = var.admin_password

  db_version = var.db_version
  sku_name   = var.sku_name
  storage_mb = var.storage_mb

  password_auth_enabled         = var.password_auth_enabled
  active_directory_auth_enabled = var.active_directory_auth_enabled

  backup_retention_days        = var.backup_retention_days
  geo_redundant_backup_enabled = var.geo_redundant_backup_enabled

  delegated_subnet_id = var.delegated_subnet_id

  environment_name = var.environment_name
  database_name    = "${var.database_name}-${var.environment_name}"

  use_shared_postgres = false
  existing_server_id  = null

  depends_on = [azurerm_resource_group.main]
}

# Langfuse database on the same PostgreSQL server (dev environment only)
resource "azurerm_postgresql_flexible_server_database" "langfuse" {
  count     = (var.enable_langfuse && var.environment_name == "dev") ? 1 : 0
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
  count                = var.use_shared_redis ? 0 : 1
  source               = "./modules/redis"
  name                 = var.redis_name
  location             = azurerm_resource_group.main.location
  resource_group_name  = azurerm_resource_group.main.name
  sku_name             = "Basic"
  family               = "C"
  capacity             = 0
  shard_count          = 0
  use_shared_redis     = false
}

# Use shared Redis outputs if enabled, otherwise use module outputs
locals {
  redis_hostname = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].hostname : module.redis[0].hostname
  redis_port     = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].port : module.redis[0].port
  redis_key      = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].primary_access_key : module.redis[0].primary_access_key
}

# ------------- Storage Resource Group (Shared across environments) -----------------------
# Use existing storage-rg resource group (created manually or by other environment)
data "azurerm_resource_group" "storage" {
  name = "storage-rg"
}

# ------------- Storage Account for Avatars (Optimized for Cost) -----------------------
resource "azurerm_storage_account" "avatars" {
  name                     = "${var.environment_name}avatarsstorage"
  resource_group_name      = data.azurerm_resource_group.storage.name
  location                = data.azurerm_resource_group.storage.location
  account_tier            = "Standard"          # Cheapest tier
  account_replication_type = "LRS"             # Cheapest replication (Local only)
  access_tier             = "Cool"             # Cool tier for cheaper storage (avatars accessed less frequently)
  
  # Enable blob public access for SAS token functionality
  allow_nested_items_to_be_public = true
  
  # Security settings
  min_tls_version                = "TLS1_2"
  https_traffic_only_enabled     = true
  
  # CORS configuration for web uploads
  blob_properties {
    cors_rule {
      allowed_headers    = ["*"]
      allowed_methods    = ["GET", "HEAD", "POST", "PUT", "DELETE"]
      allowed_origins    = ["*"]
      exposed_headers    = ["*"]
      max_age_in_seconds = 3600
    }
    
    # Delete old versions automatically to save space/cost
    delete_retention_policy {
      days = 7  # Keep deleted blobs for 7 days only (minimum)
    }
    
    # Automatically move to cheaper tiers
    versioning_enabled = false  # Disable versioning to save cost
  }

  tags = {
    Environment = var.environment_name
    ManagedBy   = "terraform"
    Purpose     = "avatars-media"
  }

  depends_on = [data.azurerm_resource_group.storage]
}

# Private container for avatars
resource "azurerm_storage_container" "avatars" {
  name                 = "avatars"
  storage_account_id   = azurerm_storage_account.avatars.id
  container_access_type = "private"

  depends_on = [azurerm_storage_account.avatars]
}

# Lifecycle management to minimize costs
resource "azurerm_storage_management_policy" "avatars_lifecycle" {
  storage_account_id = azurerm_storage_account.avatars.id

  rule {
    name    = "avatars_lifecycle"
    enabled = true
    filters {
      prefix_match = ["avatars/"]
      blob_types   = ["blockBlob"]
    }
    actions {
      base_blob {
        # Move to Cool tier after 30 days (even cheaper)
        tier_to_cool_after_days_since_modification_greater_than = 30
        # Move to Archive tier after 90 days (cheapest storage for long-term retention)
        tier_to_archive_after_days_since_modification_greater_than = 90
        # No deletion - avatars should be kept permanently
      }
    }
  }
}

# Monitoring - Diagnostic Settings for resources to Log Analytics
# Log Analytics Workspace - only create in dev environment
resource "azurerm_log_analytics_workspace" "main" {
  count               = var.environment_name == "dev" ? 1 : 0
  name                = "${var.environment_name}-laworkspace"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  daily_quota_gb = 1

  tags = {
    Environment = var.environment_name
  }
}

# Local value to determine which workspace to use (only available in dev)
locals {
  log_analytics_workspace_id = var.environment_name == "dev" ? azurerm_log_analytics_workspace.main[0].id : null
}

module "monitoring" {
  count  = var.environment_name == "dev" ? 1 : 0
  source = "./modules/monitoring"

  log_analytics_workspace_id  = local.log_analytics_workspace_id
  servicebus_namespace_id     = module.servicebus.namespace_id
  postgres_server_id          = local.use_shared_postgres ? data.azurerm_postgresql_flexible_server.shared[0].id : module.database[0].id
  signalr_id                  = module.signalr.id
  redis_id                    = var.use_shared_redis ? data.azurerm_redis_cache.shared[0].id : module.redis[0].id
  frontend_static_web_app_id  = length(var.frontend_apps) > 0 ? [for f in module.frontend : f.static_web_app_id] : []

  frontend_application_insights_ids = length(var.frontend_apps) > 0 ? [for f in module.frontend : f.application_insights_id] : []

    depends_on = [
    azurerm_log_analytics_workspace.main,
    module.servicebus,
    module.database,
    module.signalr,
    module.redis,
    module.frontend
  ]
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

module "frontend" {
  for_each = toset(var.frontend_apps)
  
  source              = "./modules/frontend"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  static_web_app_name = "${var.static_web_app_name}-${each.key}-${var.environment_name}"
  sku_tier            = var.frontend_sku_tier
  sku_size            = var.frontend_sku_size
  appinsights_retention_days = var.frontend_appinsights_retention_days
  appinsights_sampling_percentage = var.frontend_appinsights_sampling_percentage
  
  log_analytics_workspace_id = local.log_analytics_workspace_id
  
  tags = {
    Environment = var.environment_name
    Project     = "Frontend"
  }
  
  depends_on = [azurerm_resource_group.main]
}


# Reference the shared Key Vault instead of creating new ones
data "azurerm_key_vault" "shared" {
  name                = "teachin-seo-kv"
  resource_group_name = "dev-zionet-learning-2025"
}

module "clustersecretstore" {
  count       = var.environment_name == "dev" ? 1 : 0
  source     = "./modules/clustersecretstore"
  identity_id = "0997f44d-fadf-4be8-8dc6-202f7302f680" # your AKS managed identity clientId
  tenant_id   = "a814ee32-f813-4a36-9686-1b9268183e27"
}