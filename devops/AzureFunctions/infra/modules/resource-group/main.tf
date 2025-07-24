resource "azurerm_resource_group" "this" {
  name     = var.name
  location = var.location

  tags = {
    CreatedBy   = "Terraform"
  }
}


