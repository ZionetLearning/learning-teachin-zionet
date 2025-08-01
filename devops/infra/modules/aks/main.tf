# resource "azurerm_public_ip" "aks_outbound" {
#   name                = "${var.cluster_name}-outbound-pip"
#   location            = var.location
#   resource_group_name = var.resource_group_name
#   allocation_method   = "Static"
#   sku                 = "Standard"
# }

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

  # network_profile {
  #   network_plugin      = "azure"
  #   load_balancer_sku   = "standard"
  #   outbound_type       = "loadBalancer"
  #   load_balancer_profile {
  #     outbound_ip_address_ids = [azurerm_public_ip.aks_outbound.id]
  #   }
  # }
}



