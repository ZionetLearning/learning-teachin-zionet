resource "kubernetes_namespace" "grafana" {
  metadata {
    name = var.namespace
  }
}

resource "helm_release" "grafana" {
  name       = "grafana"
  repository = "https://grafana.github.io/helm-charts"
  chart      = "grafana"
  version    = var.grafana_chart_version
  namespace  = kubernetes_namespace.grafana.metadata[0].name

  values = [yamlencode({
    adminUser     = var.admin_user
    adminPassword = var.admin_password
    service = {
      type = var.service_type
      port = var.service_port
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
}

# Grafana outputs DNS and public IP

resource "azurerm_public_ip" "grafana" {
  name                = "grafana-public-ip"
  location            = var.location
  resource_group_name = var.resource_group_name
  allocation_method   = "Static"
  sku                 = "Standard"
  domain_name_label   = var.domain_name_label
}
