{{/* Registry image helper */}}
{{- define "app.image" -}}
{{- $registry := $.Values.global.dockerRegistry | default "" -}}
{{- $name := .name -}}
{{- $tag := .tag | default "latest" -}}
{{- if $registry -}}
{{ printf "%s/%s:%s" $registry $name $tag }}
{{- else -}}
{{ printf "%s:%s" $name $tag }}
{{- end -}}
{{- end -}}

{{/* Dapr annotations (app-id computed outside) */}}
{{- define "app.dapr.annotations" -}}
{{- if .enabled }}
dapr.io/enabled: "true"
dapr.io/app-id: "{{ .appId }}"
dapr.io/app-port: "{{ .appPort }}"
{{- end }}
{{- end -}}

{{/* Single source of truth for the prefix (fallback to release name) */}}
{{- define "app.prefix" -}}
{{- if .Values.global.namePrefix -}}
{{- .Values.global.namePrefix -}}
{{- else -}}
{{- .Release.Name -}}
{{- end -}}
{{- end -}}

{{/* Join prefix + suffix safely, DNS-1123 length-capped */}}
{{- define "app.join" -}}
{{- $prefix := index . 0 -}}
{{- $suffix := index . 1 -}}
{{- if $prefix -}}
{{- printf "%s-%s" $prefix $suffix | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- $suffix -}}
{{- end -}}
{{- end -}}

{{/* Namespace: if values.namespace.name is empty, use the prefix */}}
{{- define "app.namespace" -}}
{{- if .Values.namespace.name -}}
{{- .Values.namespace.name -}}
{{- else -}}
{{- include "app.prefix" . -}}
{{- end -}}
{{- end -}}

{{/* AppId = serviceName (no prefix) */}}
{{- define "app.appIdFor" -}}
{{- /* keep call signature, ignore the first arg */ -}}
{{- index . 1 -}}
{{- end -}}

{{/* Optional: Service Bus name = <prefix>-<suffix> */}}
{{- define "app.sbName" -}}
{{- $root := index . 0 -}}
{{- $suffix := index . 1 -}}
{{- include "app.join" (list (include "app.prefix" $root) $suffix) -}}
{{- end -}}
