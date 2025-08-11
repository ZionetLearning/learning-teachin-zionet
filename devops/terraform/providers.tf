terraform {
  backend "azurerm" {
    resource_group_name  = "tfstate-rg"
    storage_account_name = "teachintfstate"
    container_name       = "tfstate-aks"
    key                  = "developmentprom.terraform.tfstate"
    use_azuread_auth     = true # added because of githubactions
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3.31.0"
    }
    kubectl = {
      source  = "gavinbunney/kubectl"
      version = "~> 1.14"
    }
  }
}

provider "azurerm" {
  features {}
   #subscription_id = var.subscription_id # removed because of githubactions
   #tenant_id       = var.tenant_id # removed because of githubactions
}

provider "kubectl" {
  alias = "inherited"

  host                   = data.azurerm_kubernetes_cluster.main.kube_config[0].host
  client_certificate     = base64decode(data.azurerm_kubernetes_cluster.main.kube_config[0].client_certificate)
  client_key             = base64decode(data.azurerm_kubernetes_cluster.main.kube_config[0].client_key)
  cluster_ca_certificate = base64decode(data.azurerm_kubernetes_cluster.main.kube_config[0].cluster_ca_certificate)
}
