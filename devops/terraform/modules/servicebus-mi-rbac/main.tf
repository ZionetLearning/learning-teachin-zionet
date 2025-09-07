terraform {
  required_version = ">= 1.5.0"
}

variable "principal_id" {
  description = "Object ID (principalId) of the UAMI."
  type        = string
}

variable "namespaces" {
  description = <<EOT
List of Service Bus namespaces to grant to, each:
{ name, resource_group, assign_sender = bool, assign_receiver = bool }
EOT
  type = list(object({
    name            : string
    resource_group  : string
    assign_sender   : bool
    assign_receiver : bool
  }))
}

# Look up each namespace
data "azurerm_servicebus_namespace" "ns" {
  for_each            = { for n in var.namespaces : n.name => n }
  name                = each.value.name
  resource_group_name = each.value.resource_group
}

# Role defs on that scope
data "azurerm_role_definition" "sender" {
  for_each = data.azurerm_servicebus_namespace.ns
  name     = "Azure Service Bus Data Sender"
  scope    = each.value.id
}
data "azurerm_role_definition" "receiver" {
  for_each = data.azurerm_servicebus_namespace.ns
  name     = "Azure Service Bus Data Receiver"
  scope    = each.value.id
}

# Assignments
resource "azurerm_role_assignment" "sender" {
  for_each          = { for k, v in var.namespaces : k => v if v.assign_sender }
  scope             = data.azurerm_servicebus_namespace.ns[each.value.name].id
  role_definition_id= data.azurerm_role_definition.sender[each.value.name].role_definition_id
  principal_id      = var.principal_id
}

resource "azurerm_role_assignment" "receiver" {
  for_each          = { for k, v in var.namespaces : k => v if v.assign_receiver }
  scope             = data.azurerm_servicebus_namespace.ns[each.value.name].id
  role_definition_id= data.azurerm_role_definition.receiver[each.value.name].role_definition_id
  principal_id      = var.principal_id
}
