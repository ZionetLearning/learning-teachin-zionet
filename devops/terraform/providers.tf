terraform {
  backend "azurerm" {
    resource_group_name  = "tfstate-rg"
    storage_account_name = "teachintfstate"
    container_name       = "tfstate-aks"
    # key will be set dynamically via terraform init -backend-config
    use_azuread_auth     = true
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3.31.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.23"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 3.0.2"
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

# Kubernetes provider configuration - moved from main.tf
provider "kubernetes" {
  host                   = local.aks_kube_config.host
  client_certificate     = base64decode(local.aks_kube_config.client_certificate)
  client_key             = base64decode(local.aks_kube_config.client_key)
  cluster_ca_certificate = base64decode(local.aks_kube_config.cluster_ca_certificate)
}

# Helm provider configuration - moved from main.tf and fixed consistency
provider "helm" {
  kubernetes {
    host                   = local.aks_kube_config.host
    client_certificate     = base64decode(local.aks_kube_config.client_certificate)
    client_key             = base64decode(local.aks_kube_config.client_key)
    cluster_ca_certificate = base64decode(local.aks_kube_config.cluster_ca_certificate)
  }
}

provider "kubectl" {
  alias = "inherited"
  
  host                   = local.aks_kube_config.host
  client_certificate     = base64decode(local.aks_kube_config.client_certificate)
  client_key             = base64decode(local.aks_kube_config.client_key)
  cluster_ca_certificate = base64decode(local.aks_kube_config.cluster_ca_certificate)
}
