variable "namespace" {
  description = "Namespace for monitoring tools"
  type        = string  
}


variable "chart_version" {
  description = "Version of the Prometheus Helm chart to use"
  type        = string
  default     = "57.2.0"
}