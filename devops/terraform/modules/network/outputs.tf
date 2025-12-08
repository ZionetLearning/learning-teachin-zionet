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
  value       = try(azurerm_virtual_network.database.id, null)
  description = "ID of the database VNet"
}

output "db_vnet_name" {
  value       = try(azurerm_virtual_network.database.name, null)
  description = "Name of the database VNet"
}

output "db_subnet_id" {
  value       = try(azurerm_subnet.database.id, null)
  description = "ID of the database subnet"
}
