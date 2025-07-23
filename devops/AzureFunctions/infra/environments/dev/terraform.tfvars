# Resource Group
resource_group_name = "azurefunctions-dev-rg"
location            = "West Europe" 

# Service Bus
servicebus_namespace_name = "azurefunctions-dev-servicebus"

# Storage Account
storage_account_name              = "azfuncdevteachin"
storage_account_tier              = "Standard"
storage_account_replication_type  = "LRS"

# App Service Plan - Consumption Plan
app_service_plan_sku = {
  tier = "Dynamic"
  size = "Y1"
}

# PostgreSQL Flexible Server - Cheapest option
admin_username = "pgadmin"
admin_password = "DevStrongPassword123!" # Todo: Change this to a secure password
db_version     = "15"
sku_name    = "B_Standard_B1ms"  # Burstable Basic - cheapest option (~$15/month)
storage_mb  = 32768  # 32GB - minimum allowed

# PostgreSQL Authentication Settings
password_auth_enabled          = true
active_directory_auth_enabled  = false

backup_retention_days         = 7
geo_redundant_backup_enabled  = false



# Networking (leave empty for now - you can configure this later)
delegated_subnet_id    = null

# Database Name
database_name = "appdb-dev"