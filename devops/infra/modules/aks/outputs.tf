# output "cluster_name" {
#   value = azurerm_kubernetes_cluster.main.name
# }

# output "resource_group_name" {
#   value = var.resource_group_name
# }

# output "kube_config" {
#   value = azurerm_kubernetes_cluster.main.kube_config[0]
#   sensitive = true
# }

# # Optional: expose the host separately
# output "host" {
#   value = azurerm_kubernetes_cluster.main.kube_config[0].host
# }
# output "client_certificate" {
#   value     = azurerm_kubernetes_cluster.main.kube_config[0].client_certificate
#   sensitive = true
# }
# output "client_key" {
#   value     = azurerm_kubernetes_cluster.main.kube_config[0].client_key
#   sensitive = true
# }
# output "cluster_ca_certificate" {
#   value     = azurerm_kubernetes_cluster.main.kube_config[0].cluster_ca_certificate
#   sensitive = true
# }

# output "public_ip_address" {
#   value = azurerm_public_ip.aks_public_ip.ip_address
# }


# modules/aks/outputs.tf

output "cluster_name" {
  description = "The name of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.name
}

output "kube_config" {
  description = "Kubernetes configuration"
  value = {
    host                   = azurerm_kubernetes_cluster.main.kube_config[0].host
    client_certificate     = azurerm_kubernetes_cluster.main.kube_config[0].client_certificate
    client_key             = azurerm_kubernetes_cluster.main.kube_config[0].client_key
    cluster_ca_certificate = azurerm_kubernetes_cluster.main.kube_config[0].cluster_ca_certificate
  }
  sensitive = true
}

output "loadbalancer_public_ip" {
  description = "Static public IP for LoadBalancer services"
  value       = azurerm_public_ip.aks_loadbalancer_ip.ip_address
}

output "node_resource_group" {
  description = "The auto-generated resource group containing the AKS nodes"
  value       = azurerm_kubernetes_cluster.main.node_resource_group
}