# Network Module Outputs
# These outputs expose important network resource information for use in other modules

#--------------------- VNet Outputs ---------------------
output "vnet_id" {
  description = "ID of the Virtual Network"
  value       = azurerm_virtual_network.main.id
}

output "virtual_network_id" {
  description = "ID of the Virtual Network (alias for vnet_id for compatibility)"
  value       = azurerm_virtual_network.main.id
}

output "vnet_name" {
  description = "Name of the Virtual Network"
  value       = azurerm_virtual_network.main.name
}

output "vnet_address_space" {
  description = "Address space of the Virtual Network"
  value       = azurerm_virtual_network.main.address_space
}

output "vnet_location" {
  description = "Location of the Virtual Network"
  value       = azurerm_virtual_network.main.location
}

output "vnet_resource_group_name" {
  description = "Resource group name of the Virtual Network"
  value       = azurerm_virtual_network.main.resource_group_name
}

#--------------------- AKS Subnet Outputs ---------------------
output "aks_subnet_id" {
  description = "ID of the AKS subnet"
  value       = azurerm_subnet.aks.id
}

output "aks_subnet_name" {
  description = "Name of the AKS subnet"
  value       = azurerm_subnet.aks.name
}

output "aks_subnet_address_prefixes" {
  description = "Address prefixes of the AKS subnet"
  value       = azurerm_subnet.aks.address_prefixes
}

#--------------------- Database VNet Outputs ---------------------
output "database_vnet_id" {
  description = "ID of the database Virtual Network"
  value       = azurerm_virtual_network.database.id
}

output "database_vnet_name" {
  description = "Name of the database Virtual Network"
  value       = azurerm_virtual_network.database.name
}

output "database_vnet_address_space" {
  description = "Address space of the database Virtual Network"
  value       = azurerm_virtual_network.database.address_space
}

output "database_vnet_location" {
  description = "Location of the database Virtual Network"
  value       = azurerm_virtual_network.database.location
}

#--------------------- Database Subnet Outputs ---------------------
output "database_subnet_id" {
  description = "ID of the database subnet"
  value       = azurerm_subnet.database.id
}

output "database_subnet_name" {
  description = "Name of the database subnet"
  value       = azurerm_subnet.database.name
}

output "database_subnet_address_prefixes" {
  description = "Address prefixes of the database subnet"
  value       = azurerm_subnet.database.address_prefixes
}

#--------------------- VNet Peering Outputs ---------------------
output "main_to_database_peering_id" {
  description = "ID of the main to database VNet peering"
  value       = azurerm_virtual_network_peering.main_to_database.id
}

output "database_to_main_peering_id" {
  description = "ID of the database to main VNet peering"
  value       = azurerm_virtual_network_peering.database_to_main.id
}

# #--------------------- Database Subnet Outputs (Commented out - using public endpoints) ---------------------
# output "database_subnet_id" {
#   description = "ID of the database subnet"
#   value       = azurerm_subnet.database.id
# }

# output "database_subnet_name" {
#   description = "Name of the database subnet"
#   value       = azurerm_subnet.database.name
# }

# output "database_subnet_address_prefixes" {
#   description = "Address prefixes of the database subnet"
#   value       = azurerm_subnet.database.address_prefixes
# }

# #--------------------- Integration Subnet Outputs (Commented out - not needed yet) ---------------------
# output "integration_subnet_id" {
#   description = "ID of the integration subnet"
#   value       = azurerm_subnet.integration.id
# }

# output "integration_subnet_name" {
#   description = "Name of the integration subnet"
#   value       = azurerm_subnet.integration.name
# }

# output "integration_subnet_address_prefixes" {
#   description = "Address prefixes of the integration subnet"
#   value       = azurerm_subnet.integration.address_prefixes
# }

# #--------------------- Management Subnet Outputs (Commented out - not needed yet) ---------------------
# output "management_subnet_id" {
#   description = "ID of the management subnet"
#   value       = azurerm_subnet.management.id
# }

# output "management_subnet_name" {
#   description = "Name of the management subnet"
#   value       = azurerm_subnet.management.name
# }

# output "management_subnet_address_prefixes" {
#   description = "Address prefixes of the management subnet"
#   value       = azurerm_subnet.management.address_prefixes
# }

#--------------------- All Subnets Summary ---------------------
output "all_subnet_ids" {
  description = "Map of all subnet IDs for easy reference"
  value = {
    aks      = azurerm_subnet.aks.id
    database = azurerm_subnet.database.id
  }
}

output "all_subnet_names" {
  description = "Map of all subnet names for easy reference"
  value = {
    aks      = azurerm_subnet.aks.name
    database = azurerm_subnet.database.name
  }
}
