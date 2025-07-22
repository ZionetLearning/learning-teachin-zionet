# Resource Group
resource_group_name = "prod-rg"
location            = "West Europe"

# Service Bus
servicebus_namespace_name = "prod-sb"

# Storage Account
storage_account_name              = "prodstorageacct"
storage_account_tier              = "Standard"
storage_account_replication_type  = "LRS"

# App Service Plan
app_service_plan_sku = "Y1" # Consumption Plan

# PostgreSQL Flexible Server
admin_username = "pgadmin"
#admin_password = "VerySecure123!"

version     = "15"
sku_name    = "B1ms"
storage_mb  = 32768

password_auth_enabled          = true
active_directory_auth_enabled  = false

backup_retention_days         = 7
geo_redundant_backup_enabled  = false

high_availability_mode = "Disabled"
delegated_subnet_id    = null

database_name = "appdb-prod"