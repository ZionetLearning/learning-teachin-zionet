terraform {
  required_providers {
    kubectl = {
      source  = "gavinbunney/kubectl"
      version = "~> 1.14.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.26"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.13"
    }
  }
}

provider "kubectl" {
  config_path = "~/.kube/config"
}

provider "kubernetes" {
  config_path = "~/.kube/config"
}

provider "helm" {
  kubernetes {
    config_path = "~/.kube/config"
  }
}

variable "docker_registry" {
  type = string
}

locals {
  yaml_files = fileset("${path.module}/../kubernetes", "*.yaml")
  dapr_components  = fileset("${path.module}/../kubernetes/dapr/components", "*.yaml")
  dapr_config_file = "${path.module}/../kubernetes/dapr/config.yaml"
}

resource "kubernetes_namespace" "devops_model" {
  metadata {
    name = "devops-model"
  }
}

resource "kubernetes_secret" "signalr_connection" {
  metadata {
    name      = "dapr-secretstore"
    namespace = kubernetes_namespace.devops_model.metadata[0].name
  }
  data = {
    SignalRConnectionString = "Endpoint=http://host.docker.internal:8888;Port=8888;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;"
  }
  type = "Opaque"
  depends_on = [kubernetes_namespace.devops_model]
}

resource "kubernetes_secret" "cosmosdb_connection" {
  metadata {
    name      = "cosmosdb-connection"
    namespace = kubernetes_namespace.devops_model.metadata[0].name
  }
  data = {
    CosmosDbConnectionString = "AccountEndpoint=https://cosmosdb-emulator:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;"
  }
  type = "Opaque"
  depends_on = [kubernetes_namespace.devops_model]
}

resource "kubernetes_secret" "cosmosdb_cert" {
  metadata {
    name      = "cosmosdb-cert"
    namespace = kubernetes_namespace.devops_model.metadata[0].name
  }
  data = {
    "cosmosdbemulator.crt" = filebase64("${path.module}/../kubernetes/cosmosdbemulator.crt")
  }
  type = "Opaque"
  depends_on = [kubernetes_namespace.devops_model]
}

resource "kubectl_manifest" "namespace" {
  yaml_body = file("${path.module}/../kubernetes/namespace-model.yaml")
  depends_on = [kubernetes_namespace.devops_model]
}

resource "kubectl_manifest" "dapr_system_ns" {
  yaml_body = <<YAML
apiVersion: v1
kind: Namespace
metadata:
  name: dapr-system
YAML
}

resource "helm_release" "dapr" {
  name             = "dapr"
  namespace        = "dapr-system"
  repository       = "https://dapr.github.io/helm-charts/"
  chart            = "dapr"
  version          = "1.14.4"
  create_namespace = false
  depends_on       = [kubectl_manifest.dapr_system_ns]
}

resource "null_resource" "wait_for_dapr_control_plane" {
  depends_on = [helm_release.dapr]
  provisioner "local-exec" {
    command = <<EOT
echo for (\$i=0; \$i -lt 30; \$i++) {>> wait-for-dapr.ps1
echo   if ((kubectl get pods -n dapr-system ^| Select-String 'sidecar-injector')) { exit 0 }>> wait-for-dapr.ps1
echo   Write-Host "Waiting for Dapr control plane to be available...">> wait-for-dapr.ps1
echo   Start-Sleep -Seconds 2>> wait-for-dapr.ps1
echo }>> wait-for-dapr.ps1
echo Write-Error "Timeout waiting for Dapr control plane">> wait-for-dapr.ps1
echo exit 1>> wait-for-dapr.ps1
powershell -ExecutionPolicy Bypass -File wait-for-dapr.ps1
del wait-for-dapr.ps1
EOT
  }
}

resource "kubectl_manifest" "dapr_config" {
  yaml_body  = file(local.dapr_config_file)
  depends_on = [
    null_resource.wait_for_dapr_control_plane,
    kubectl_manifest.namespace,
    kubernetes_secret.signalr_connection,
    kubernetes_secret.cosmosdb_connection,
    kubernetes_secret.cosmosdb_cert
  ]
}

resource "kubectl_manifest" "dapr_components" {
  for_each    = { for f in local.dapr_components : f => f }
  yaml_body   = file("${path.module}/../kubernetes/dapr/components/${each.value}")
  depends_on  = [
    kubectl_manifest.dapr_config,
    kubernetes_secret.signalr_connection,
    kubernetes_secret.cosmosdb_connection,
    kubernetes_secret.cosmosdb_cert
  ]
}

resource "kubectl_manifest" "all_manifests" {
  for_each = { for f in local.yaml_files : f => f }
  yaml_body = templatefile("${path.module}/../kubernetes/${each.value}", {
    DOCKER_REGISTRY = var.docker_registry
  })
  depends_on = [kubectl_manifest.dapr_components]
}
