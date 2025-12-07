output "vnet_id" {
  value       = azurerm_virtual_network.main.id
  description = "ID of the main AKS VNet"
}

output "vnet_name" {
  value       = azurerm_virtual_network.main.name
  description = "Name of the main AKS VNet"
}

output "aks_subnet_id" {
  value       = azurerm_subnet.aks.id
  description = "ID of the AKS subnet"
}

output "db_vnet_id" {
  value       = try(azurerm_virtual_network.database[0].id, null)
  description = "ID of the database VNet (null if enable_db_vnet=false)"
}

output "db_vnet_name" {
  value       = try(azurerm_virtual_network.database[0].name, null)
  description = "Name of the database VNet (null if enable_db_vnet=false)"
}

output "db_subnet_id" {
  value       = try(azurerm_subnet.database[0].id, null)
  description = "ID of the database subnet (null if enable_db_vnet=false)"
}

output "aks_nsg_id" {
  value       = azurerm_network_security_group.aks.id
  description = "ID of the AKS network security group"
}
