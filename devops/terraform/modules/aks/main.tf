

resource "azurerm_user_assigned_identity" "aks" {
  name                = "${var.prefix}-aks-uami"
  resource_group_name = var.resource_group_name
  location            = var.location
}
resource "azurerm_kubernetes_cluster" "main" {
  name                = var.cluster_name
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "${var.cluster_name}-dns"

  default_node_pool {
    name                 = "default"
    vm_size              = var.stable_vm_size
    auto_scaling_enabled = true
    min_count            = var.stable_min_node_count
    max_count            = var.stable_max_node_count
    
    # System pods and critical workloads
    node_labels = {
      "node-type" = "stable"
      "workload"  = "system"
    }
    
    # Prevent eviction of stable nodes
    only_critical_addons_enabled = false
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.aks.id]
  }
  
  oidc_issuer_enabled = true # Enable OIDC issuer for the cluster

}

# Spot instance node pool for cost-effective workloads
resource "azurerm_kubernetes_cluster_node_pool" "spot" {
  name                  = "spot"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  vm_size               = var.spot_vm_size
  
  # Auto-scaling configuration
  auto_scaling_enabled = true
  min_count            = var.spot_min_node_count
  max_count            = var.spot_max_node_count
  
  # Spot instance configuration
  priority        = "Spot"
  eviction_policy = "Delete"
  spot_max_price  = var.spot_max_price # -1 means pay up to on-demand price
  
  # Labels and taints for spot instances
  node_labels = {
    "node-type"            = "spot"
    "kubernetes.azure.com/scalesetpriority" = "spot"
  }
  
  node_taints = [
    "kubernetes.azure.com/scalesetpriority=spot:NoSchedule"
  ]
  
  tags = {
    Environment = var.prefix
    NodeType    = "spot"
  }
}