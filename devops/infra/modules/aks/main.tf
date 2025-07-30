# resource "azurerm_public_ip" "aks_public_ip" {
#   name                = "${var.cluster_name}-public-ip"
#   location            = var.location
#   resource_group_name = var.resource_group_name
#   allocation_method   = "Static"
#   sku                 = "Standard"
# }

# resource "azurerm_kubernetes_cluster" "main" {
#   name                = var.cluster_name
#   location            = var.location
#   resource_group_name = var.resource_group_name
#   dns_prefix          = "${var.cluster_name}-dns"

#   default_node_pool {
#     name       = "default"
#     node_count = var.node_count
#     vm_size    = var.vm_size
#   }

#   identity {
#     type = "SystemAssigned"
#   }

#   network_profile {
#     network_plugin     = "kubenet"
#     load_balancer_sku  = "standard"
#     outbound_type      = "loadBalancer"
#     # load_balancer_profile {
#     #   outbound_ip_address_ids = [azurerm_public_ip.aks_public_ip.id]
#     # }
#   }

#   # add other options as needed
# }



#---------
# modules/aks/main.tf

# Get the node resource group name (will be created by AKS)
locals {
  node_resource_group = "MC_${var.resource_group_name}_${var.cluster_name}_${var.location}"
}

# Create static public IP in the NODE resource group (not your main RG)
resource "azurerm_public_ip" "aks_loadbalancer_ip" {
  name                = "${var.cluster_name}-loadbalancer-ip"
  location            = var.location
  resource_group_name = local.node_resource_group
  allocation_method   = "Static"
  sku                 = "Standard"
  
  # This depends_on ensures the AKS cluster creates the node RG first
  depends_on = [azurerm_kubernetes_cluster.main]
}

resource "azurerm_kubernetes_cluster" "main" {
  name                = var.cluster_name
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "${var.cluster_name}-dns"

  default_node_pool {
    name       = "default"
    node_count = var.node_count
    vm_size    = var.vm_size
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin     = "kubenet"
    load_balancer_sku  = "standard"
    outbound_type      = "loadBalancer"
    
    # Optional: If you also want static outbound IPs (different from LoadBalancer IP)
    # load_balancer_profile {
    #   outbound_ip_address_ids = [azurerm_public_ip.aks_outbound_ip.id]
    # }
  }

}

# Grant the AKS cluster identity permissions to manage the public IP
resource "azurerm_role_assignment" "aks_network_contributor" {
  scope                = azurerm_public_ip.aks_loadbalancer_ip.id
  role_definition_name = "Network Contributor"
  principal_id         = azurerm_kubernetes_cluster.main.identity[0].principal_id
}

# Optional: Create a separate outbound IP if needed
# resource "azurerm_public_ip" "aks_outbound_ip" {
#   name                = "${var.cluster_name}-outbound-ip"
#   location            = var.location
#   resource_group_name = local.node_resource_group
#   allocation_method   = "Static"
#   sku                 = "Standard"
#   depends_on         = [azurerm_kubernetes_cluster.main]
# }