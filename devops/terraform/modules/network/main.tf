# Network Module - Main Resources
# This module creates the core network infrastructure with VNet and subnets
# Following Azure and Terraform best practices for network segmentation
#--------------------- Virtual Network ---------------------
resource "azurerm_virtual_network" "main" {
  name                = var.vnet_name
  location            = var.location
  resource_group_name = var.resource_group_name
  address_space       = var.address_space

  # Optional DNS servers configuration
  dns_servers = length(var.dns_servers) > 0 ? var.dns_servers : null

  # DDoS protection configuration (typically enabled for production)
  # Note: DDoS protection requires a separate DDoS protection plan resource
  # For now, we'll comment this out as it requires additional setup
  # dynamic "ddos_protection_plan" {
  #   for_each = var.enable_ddos_protection ? [1] : []
  #   content {
  #     id     = azurerm_network_ddos_protection_plan.main[0].id
  #     enable = true
  #   }
  # }

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

  # Enable delegation for AKS if needed (typically not required for basic setups)
  # delegation {
  #   name = "aks-delegation"
  #   service_delegation {
  #     name = "Microsoft.ContainerService/managedClusters"
  #   }
  # }
}

#--------------------- Database VNet ---------------------
# Separate VNet for database services in different region
resource "azurerm_virtual_network" "database" {
  name                = var.db_vnet_name
  location            = var.db_vnet_location
  resource_group_name = var.resource_group_name
  address_space       = var.db_vnet_address_space

  # Optional DNS servers configuration
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

  # Delegation for PostgreSQL Flexible Server - > RESERVED FOR POSTGRESQL ONLY
  delegation {
    name = "postgresql-delegation"
    service_delegation {
      name    = "Microsoft.DBforPostgreSQL/flexibleServers"
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
  resource_group_name       =  var.resource_group_name
  virtual_network_name      = azurerm_virtual_network.database.name
  remote_virtual_network_id = azurerm_virtual_network.main.id

  allow_virtual_network_access = true
  allow_forwarded_traffic      = true
  allow_gateway_transit        = false
  use_remote_gateways          = false
}



# #--------------------- Integration Subnet ---------------------
# # Subnet for integration services and private endpoints
# # This subnet will host private endpoints for various Azure services
# resource "azurerm_subnet" "integration" {
#   name                 = var.integration_subnet_name
#   resource_group_name  = var.resource_group_name
#   virtual_network_name = azurerm_virtual_network.main.name
#   address_prefixes     = [var.integration_subnet_prefix]

#   # Disable private endpoint network policies to allow private endpoints
#   # Note: This is the correct attribute name for azurerm provider 4.x
#   private_endpoint_network_policies = "Disabled"
# }

# #--------------------- Management Subnet ---------------------
# # Subnet for management and monitoring services
# # This subnet can host jump boxes, monitoring tools, etc.
# resource "azurerm_subnet" "management" {
#   name                 = var.management_subnet_name
#   resource_group_name  = var.resource_group_name
#   virtual_network_name = azurerm_virtual_network.main.name
#   address_prefixes     = [var.management_subnet_prefix]
# }
