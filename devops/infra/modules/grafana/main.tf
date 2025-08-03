resource "kubernetes_namespace" "grafana" {
  metadata {
    name = var.namespace
  }
}


resource "azurerm_public_ip" "grafana" {
  name                = "grafana-public-ip"
  location            = var.location
  resource_group_name = var.resource_group_name
  allocation_method   = "Static"
  sku                 = "Standard"
  domain_name_label   = var.domain_name_label
}


resource "helm_release" "grafana" {
  name       = "grafana"
  repository = "https://grafana.github.io/helm-charts"
  chart      = "grafana"
  version    = var.grafana_chart_version
  namespace  = kubernetes_namespace.grafana.metadata[0].name

  timeout = 600

  values = [yamlencode({
    adminUser     = var.admin_user
    adminPassword = var.admin_password
    service = {
      type            = var.service_type
      port            = var.service_port
      loadBalancerIP  = azurerm_public_ip.grafana.ip_address # for dns and public IP
    }
    sidecar = {
      dashboards = {
        enabled         = var.sidecar_dashboards
        searchNamespace = var.namespace
      }

      datasources = {
        enabled         = true
        searchNamespace = var.namespace
      }
    }

    persistence = {
      enabled          = var.persistence_enabled
      type             = "pvc"
      accessModes      = var.persistence_access_modes
      size             = var.persistence_size
      storageClassName = var.persistence_storage_class != "" ? var.persistence_storage_class : null
    }
  })]
  depends_on = [
    kubernetes_namespace.grafana,
    azurerm_public_ip.grafana
  ]
}


