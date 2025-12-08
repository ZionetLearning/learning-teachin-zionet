#--------------------- Main Virtual Network (AKS) ---------------------
# VNet for AKS cluster
resource "azurerm_virtual_network" "main" {
  name                = var.vnet_name
  location            = var.location
  resource_group_name = var.resource_group_name
  address_space       = var.address_space

  dns_servers = length(var.dns_servers) > 0 ? var.dns_servers : null

  tags = merge(var.tags, {
    Name = var.vnet_name
    Type = "Virtual Network"
  })
}

#--------------------- AKS Subnet ---------------------
# Dedicated subnet for Azure Kubernetes Service
# This subnet will host AKS nodes and pods
resource "azurerm_subnet" "aks" {
  name                 = var.aks_subnet_name
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = [var.aks_subnet_prefix]

  service_endpoints = [
    "Microsoft.ContainerRegistry",
    "Microsoft.Storage",
    "Microsoft.KeyVault"
  ]
}

#--------------------- Database VNet ---------------------
# Separate VNet for database services in different region
resource "azurerm_virtual_network" "database" {
  name                = var.db_vnet_name
  location            = var.db_vnet_location
  resource_group_name = var.resource_group_name
  address_space       = var.db_vnet_address_space

  dns_servers = length(var.dns_servers) > 0 ? var.dns_servers : null

  tags = merge(var.tags, {
    Name = var.db_vnet_name
    Type = "Database Virtual Network"
  })
}

#--------------------- Database Subnet ---------------------
# Dedicated subnet for database services (PostgreSQL, etc.)
# This subnet is delegated to database services for private connectivity
resource "azurerm_subnet" "database" {
  name                 = var.db_subnet_name
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.database.name
  address_prefixes     = [var.db_subnet_prefix]
 
  # Delegation for PostgreSQL Flexible Server -> RESERVED FOR POSTGRESQL ONLY
  delegation {
    name = "postgresql-delegation"
    service_delegation {
      name = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action",
      ]
    }
  }
}

#--------------------- VNet Peering ---------------------
# Bidirectional peering between main VNet and database VNet
# This enables communication between AKS and database across regions
resource "azurerm_virtual_network_peering" "main_to_database" {
  name                      = "${var.vnet_name}-to-${var.db_vnet_name}"
  resource_group_name       = var.resource_group_name
  virtual_network_name      = azurerm_virtual_network.main.name
  remote_virtual_network_id = azurerm_virtual_network.database.id
  allow_virtual_network_access = true
  allow_forwarded_traffic      = true
  allow_gateway_transit        = false
  use_remote_gateways          = false
}

resource "azurerm_virtual_network_peering" "database_to_main" {
  name                      = "${var.db_vnet_name}-to-${var.vnet_name}"
  resource_group_name       = var.resource_group_name
  virtual_network_name      = azurerm_virtual_network.database.name
  remote_virtual_network_id = azurerm_virtual_network.main.id

  allow_virtual_network_access = true
  allow_forwarded_traffic      = true
  allow_gateway_transit        = false
  use_remote_gateways          = false
}

#--------------------- Network Security Group for AKS ---------------------
# Basic NSG to allow outbound traffic and inbound from control plane
# resource "azurerm_network_security_group" "aks" {
#   name                = "${var.aks_subnet_name}-nsg"
#   location            = var.location
#   resource_group_name = var.resource_group_name

#   tags = merge(var.tags, {
#     Name = "${var.aks_subnet_name}-nsg"
#   })
# }

# # Allow outbound to internet (default for AKS)
# resource "azurerm_network_security_rule" "allow_outbound_internet" {
#   name                        = "AllowOutboundInternet"
#   priority                    = 100
#   direction                   = "Outbound"
#   access                      = "Allow"
#   protocol                    = "*"
#   source_port_range           = "*"
#   destination_port_range      = "*"
#   source_address_prefix       = "*"
#   destination_address_prefix  = "*"
#   resource_group_name         = var.resource_group_name
#   network_security_group_name = azurerm_network_security_group.aks.name
# }

# # Allow inbound from same subnet (node-to-node communication)
# resource "azurerm_network_security_rule" "allow_inbound_subnet" {
#   name                        = "AllowInboundSubnet"
#   priority                    = 101
#   direction                   = "Inbound"
#   access                      = "Allow"
#   protocol                    = "*"
#   source_port_range           = "*"
#   destination_port_range      = "*"
#   source_address_prefix       = var.aks_subnet_prefix
#   destination_address_prefix  = var.aks_subnet_prefix
#   resource_group_name         = var.resource_group_name
#   network_security_group_name = azurerm_network_security_group.aks.name
# }

# # Associate NSG with AKS subnet
# resource "azurerm_subnet_network_security_group_association" "aks" {
#   subnet_id                 = azurerm_subnet.aks.id
#   network_security_group_id = azurerm_network_security_group.aks.id
# }
