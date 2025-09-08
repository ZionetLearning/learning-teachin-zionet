resource "kubernetes_manifest" "cluster_secret_store" {
  manifest = {
    apiVersion = "external-secrets.io/v1"
    kind       = "ClusterSecretStore"
    metadata = {
      name = "azure-keyvault-backend"
    }
    spec = {
      provider = {
        azurekv = {
          authType  = "ManagedIdentity"
          vaultUrl  = "https://teachin-seo-kv.vault.azure.net/"
          identityId = var.identity_id
          tenantId   = var.tenant_id
        }
      }
    }
  }
}
