#id
output "id" {
  value = azurerm_service_plan.this.id
}

output "app_service_plan_id" {
  value = azurerm_service_plan.this.id
}
#name
output "app_service_plan_name" {
    value = azurerm_service_plan.this.name
}