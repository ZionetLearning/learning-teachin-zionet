
resource "azurerm_kubernetes_cluster" "main" {
  name                = var.cluster_name
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "${var.cluster_name}-dns"

  default_node_pool {
    name       = "default"
    vm_size    = var.vm_size
    max_count = var.max_node_count
    min_count = var.min_node_count
    auto_scaling_enabled  = true
  }

  identity {
    type = "SystemAssigned"
  }


}


