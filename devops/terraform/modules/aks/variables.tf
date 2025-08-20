variable "resource_group_name" {
  type = string
}
variable "location" {
  type = string
}
variable "cluster_name" {
  type = string
}
variable "node_count" {
  type    = number
  default = 2
}
variable "max_node_count" {
  type    = number
  default = 3
}
variable "min_node_count" {
  type    = number
  default = 1
}
variable "vm_size" {
  type    = string
  default = "Standard_B2s"
}

