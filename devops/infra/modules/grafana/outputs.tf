output "grafana_admin_password" {
  description = "Grafana admin password"
  value       = var.admin_password
}

output "namespace" {
  description = "Grafana namespace"
  value       = var.namespace
}

# Grafana outputs DNS and public IP

output "public_ip_address" {
  value       = azurerm_public_ip.grafana.ip_address
  description = "Grafana Public IP address"
}

output "dns_name" {
  value       = azurerm_public_ip.grafana.fqdn
  description = "Grafana DNS name"
}
