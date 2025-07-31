###############################################################################
# Module: k8s_manifests
# Purpose: Apply secrets, Dapr components, services & deployments
#          found in the given k8s folder hierarchy.
###############################################################################
terraform {
  required_providers {
    kubectl = {
      source  = "gavinbunney/kubectl"
      version = "~> 1.14"
      configuration_aliases = [
        kubectl.inherited
      ]
    }
  }
}

###############################################################################
#  Inputs
###############################################################################
variable "k8s_dir" {
  description = "Root folder that contains secrets/, dapr/, services/, deployments/ sub-folders"
  type        = string
}

variable "docker_registry" {
  description = "Org/registry to prepend to image names inside deployments"
  type        = string
}

###############################################################################
#  Local helpers
###############################################################################
locals {
  secret_files      = fileset("${var.k8s_dir}/secrets",     "*.yaml")
  dapr_files        = fileset("${var.k8s_dir}/dapr/components",        "*.yaml")
  service_files     = fileset("${var.k8s_dir}/services",    "*.yaml")
  deployment_files  = fileset("${var.k8s_dir}/deployments", "*.yaml")

  rendered_deployments = {
    for f in local.deployment_files :
    f => templatefile("${var.k8s_dir}/deployments/${f}",
           { DOCKER_REGISTRY = var.docker_registry })
  }
}

###############################################################################
#  Apply secrets / components / services verbatim
###############################################################################
resource "kubectl_manifest" "secrets" {
  provider  = kubectl.inherited
  for_each  = { for f in local.secret_files : f => file("${var.k8s_dir}/secrets/${f}") }
  yaml_body = each.value
  depends_on = [var.namespace]
}

resource "kubectl_manifest" "dapr_components" {
  provider  = kubectl.inherited
  for_each  = { for f in local.dapr_files : f => file("${var.k8s_dir}/dapr/${f}") }
  yaml_body = each.value
  depends_on = [var.namespace]
}

resource "kubectl_manifest" "services" {
  provider  = kubectl.inherited
  for_each  = { for f in local.service_files : f => file("${var.k8s_dir}/services/${f}") }
  yaml_body = each.value
  depends_on = [var.namespace]
}

###############################################################################
#  Apply deployments with ${DOCKER_REGISTRY} substitution
###############################################################################
resource "kubectl_manifest" "deployments" {
  provider  = kubectl.inherited
  for_each  = local.rendered_deployments
  yaml_body = each.value
  depends_on = [var.namespace]
}
