output "cluster_name" {
  value = azurerm_kubernetes_cluster.main.name
}

output "resource_group_name" {
  value = var.resource_group_name
}

output "kube_config" {
  value = azurerm_kubernetes_cluster.main.kube_config[0]
  sensitive = true
}

# Optional: expose the host separately
output "host" {
  value = azurerm_kubernetes_cluster.main.kube_config[0].host
}
output "client_certificate" {
  value     = azurerm_kubernetes_cluster.main.kube_config[0].client_certificate
  sensitive = true
}
output "client_key" {
  value     = azurerm_kubernetes_cluster.main.kube_config[0].client_key
  sensitive = true
}
output "cluster_ca_certificate" {
  value     = azurerm_kubernetes_cluster.main.kube_config[0].cluster_ca_certificate
  sensitive = true
}

output "public_ip_address" {
  value = azurerm_public_ip.aks_outbound_ip.ip_address
}

