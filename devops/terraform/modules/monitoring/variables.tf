variable "log_analytics_workspace_id" {
  type = string
}

variable "servicebus_namespace_id" {
  type = string
}

variable "postgres_server_id" {
  type = string
}

variable "signalr_id" {
  type = string
}

variable "redis_id" {
  type = string
}

variable "frontend_static_web_app_id" {
  type = list(string)
}

variable "frontend_application_insights_ids" {
  description = "List of Application Insights IDs for frontend apps"
  type        = list(string)
  default     = []
}