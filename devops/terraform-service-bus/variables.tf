variable "location" {
  type    = string
  default = "Israel Central"
}

variable "resource_group_name" {
  type    = string
  default = "sb-queue-test"
}

variable "namespace_name" {
  type    = string
  default = "sb-dev-shared-queue"
}

variable "queue_name" {
  type    = string
  default = "incoming-queue"
}
