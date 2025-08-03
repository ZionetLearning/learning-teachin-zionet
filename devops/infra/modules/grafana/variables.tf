variable "namespace" {
  description = "Kubernetes namespace for Grafana"
  type        = string
  default     = "devops-logs"
}

variable "admin_user" {
  description = "Grafana admin username"
  type        = string
  default     = "admin"
}

variable "admin_password" {
  description = "Grafana admin password"
  type        = string
  default     = "admin123"
}

variable "service_type" {
  description = "Type of Grafana service (LoadBalancer or NodePort)"
  type        = string
  default     = "LoadBalancer"
}

variable "service_port" {
  description = "Port for the Grafana service"
  type        = number
  default     = 80
}

variable "sidecar_dashboards" {
  description = "Enable Grafana sidecar dashboards"
  type        = bool
  default     = true
}

variable "persistence_enabled" {
  description = "Enable persistent storage for Grafana"
  type        = bool
  default     = true
}

variable "persistence_size" {
  description = "Size of the persistent volume for Grafana"
  type        = string
  default     = "5Gi"
}

variable "persistence_storage_class" {
  description = "Storage class name for the PVC. Leave empty for default"
  type        = string
  default     = ""
}

variable "persistence_access_modes" {
  description = "Access modes for the PVC"
  type        = list(string)
  default     = ["ReadWriteOnce"]
}

variable "persistence_finalizers" {
  description = "Finalizers for the PVC"
  type        = list(string)
  default     = ["retain"]
}

variable "grafana_chart_version" {
  description = "Grafana Helm chart version"
  type        = string
  default     = "7.3.8"
}

# Grafana outputs DNS and public IP

variable "location" {
  description = "Azure region"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "domain_name_label" {
  description = "DNS label for Grafana public IP"
  type        = string
  default     = "grafana-demo"
}

# mc_resource_group
variable "mc_resource_group" {
  description = "Resource group for the managed cluster"
  type        = string
}