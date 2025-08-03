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

# Connection strings for application deployment
output "servicebus_connection_string" {
  value     = module.servicebus.connection_string
  sensitive = true
}

output "postgres_connection_string" {
  value     = module.database.postgres_connection_string
  sensitive = true
}

output "signalr_connection_string" {
  value     = module.signalr.primary_connection_string
  sensitive = true
}

output "grafana_admin_password" {
  description = "Grafana admin password"
  value       = module.grafana.grafana_admin_password
}

output "grafana_namespace" {
  description = "Grafana namespace"
  value       = module.grafana.namespace
}

# Grafana outputs DNS and public IP

output "grafana_dns_name" {
  value       = module.grafana.dns_name
  description = "Grafana DNS name"
}
output "grafana_public_ip" {
  value       = module.grafana.public_ip_address
  description = "Grafana Public IP address"
}
