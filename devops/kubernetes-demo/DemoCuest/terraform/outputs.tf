output "aks_cluster_name" {
  value = module.aks.cluster_name
}

output "aks_resource_group" {
  value = module.aks.resource_group_name
}

output "aks_kube_config" {
  value     = module.aks.kube_config
  sensitive = true
}

# Optionally output host/certs for use in providers or scripts
output "aks_host" {
  value     = module.aks.kube_config.host
  sensitive = true
}

output "aks_client_certificate" {
  value     = module.aks.client_certificate
  sensitive = true
}
output "aks_client_key" {
  value     = module.aks.client_key
  sensitive = true
}
output "aks_cluster_ca_certificate" {
  value     = module.aks.cluster_ca_certificate
  sensitive = true
}

# Example: output namespace for reference
output "namespace_model" {
  value = kubernetes_namespace.model.metadata[0].name
}
