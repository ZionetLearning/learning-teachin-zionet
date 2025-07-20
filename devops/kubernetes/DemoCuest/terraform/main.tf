terraform {
  required_providers {
    kubectl = {
      source  = "gavinbunney/kubectl"
      version = "~> 1.14.0"
    }
  }
}

provider "kubectl" {
  config_path = "~/.kube/config"
}

variable "docker_registry" {
  type = string
}

locals {
  yaml_files = fileset("${path.module}/../kubernetes", "*.yaml")
}

resource "kubectl_manifest" "all_manifests" {
  for_each = { for f in local.yaml_files : f => f }

  yaml_body = templatefile("${path.module}/../kubernetes/${each.value}", {
    DOCKER_REGISTRY = var.docker_registry
  })

  depends_on = [null_resource.dapr_init]
}

resource "null_resource" "dapr_init" {
  provisioner "local-exec" {

    command = "dapr init -k; kubectl apply -f todoqueue.yaml -n devops-model; kubectl apply -f clientcallback.yaml -n devops-model; kubectl apply -f clientresponsequeue.yaml -n devops-model; kubectl apply -f todomanagercallbackqueue.yaml -n devops-model;"
  }
}