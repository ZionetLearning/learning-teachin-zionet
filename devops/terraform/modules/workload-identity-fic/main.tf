terraform {
  required_version = ">= 1.5.0"
}

variable "uami_id" {
  description = "Resource ID of the User Assigned Managed Identity to bind."
  type        = string
}

variable "oidc_issuer_url" {
  description = "AKS OIDC issuer (from azurerm_kubernetes_cluster.oidc_issuer_url)."
  type        = string
}

variable "bindings" {
  description = "List of { namespace, serviceaccount, name_suffix? } to bind."
  type = list(object({
    namespace      : string
    serviceaccount : string
    name_suffix    : optional(string) # optional friendly suffix for the FIC name
  }))
}

locals {
  map = {
    for b in var.bindings :
    "${b.namespace}/${b.serviceaccount}" => b
  }
}

resource "azurerm_federated_identity_credential" "fic" {
  for_each            = local.map
  name                = "${replace(each.value.namespace, "/", "-")}-${each.value.serviceaccount}-${coalesce(each.value.name_suffix, "fic")}"
  parent_id           = var.uami_id
  resource_group_name = "" # filled by Azure from parent_id; leaving empty works across providers
  issuer              = var.oidc_issuer_url
  audience            = ["api://AzureADTokenExchange"]
  subject             = "system:serviceaccount:${each.value.namespace}:${each.value.serviceaccount}"
}
