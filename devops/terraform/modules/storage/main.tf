# ------------- Storage Resource Group (Shared across environments) -----------------------
# Use existing storage-rg resource group (created manually or by other environment)
data "azurerm_resource_group" "storage" {
  name = var.name
}

# ------------- Storage Account for Avatars (Optimized for Cost) -----------------------
resource "azurerm_storage_account" "avatars" {
  name                     = "${var.environment_name}avatarsstorage"
  resource_group_name      = data.azurerm_resource_group.storage.name
  location                = data.azurerm_resource_group.storage.location
  account_tier            = var.account_tier          # Cheapest tier
  account_replication_type = var.account_replication_type # Cheapest replication (Local only)
  access_tier             = var.access_tier             # Cool tier for cheaper storage (avatars accessed less frequently)

  # Enable blob public access for SAS token functionality
  allow_nested_items_to_be_public = var.allow_nested_items_to_be_public
  
  # Security settings
  min_tls_version                = var.min_tls_version
  https_traffic_only_enabled     = var.https_traffic_only_enabled

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
      days = var.days  # Keep deleted blobs for 7 days only (minimum)
    }
    
    # Automatically move to cheaper tiers
    versioning_enabled = var.versioning_enabled  # Disable versioning to save cost
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
  container_access_type = var.container_access_type

  depends_on = [azurerm_storage_account.avatars]
}

# Lifecycle management to minimize costs
resource "azurerm_storage_management_policy" "avatars_lifecycle" {
  storage_account_id = azurerm_storage_account.avatars.id

  rule {
    name    = "avatars_lifecycle"
    enabled = var.enabled
    filters {
      prefix_match = ["avatars/"]
      blob_types   = ["blockBlob"]
    }
    actions {
      base_blob {
        # Move to Cool tier after 30 days (even cheaper)
        tier_to_cool_after_days_since_modification_greater_than = var.tier_to_cool_after_days_since_modification_greater_than
        # Move to Archive tier after 90 days (cheapest storage for long-term retention)
        tier_to_archive_after_days_since_modification_greater_than = var.tier_to_archive_after_days_since_modification_greater_than
        # No deletion - avatars should be kept permanently
      }
    }
  }
}