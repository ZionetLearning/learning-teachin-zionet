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
}
