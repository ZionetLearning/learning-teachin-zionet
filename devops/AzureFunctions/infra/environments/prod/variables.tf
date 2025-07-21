variable "resource_group_name" {
    description = "Name of the resource group"
    type        = string
}

variable "location" {
    description = "Azure region for the resources"
    type        = string
}

variable "servicebus_namespace_name" {
    description = "Name of the Service Bus Namespace"
    type        = string
}

# storage_account_name
variable "storage_account_name" {
    description = "Name of the Storage Account"
    type        = string
}

# storage_account_tier 
variable "storage_account_tier" {
    description = "Tier of the Storage Account (e.g., Standard, Premium)"
    type        = string
}

#storage_account_replication_type
variable "storage_account_replication_type" {
    description = "Replication type of the Storage Account (e.g., LRS, GRS, ZRS)"
    type        = string
}

# app_service_plan_sku
variable "app_service_plan_sku" {
    description = "SKU for the App Service Plan"
    type = object({
        tier = string
        size = string
    })
}
