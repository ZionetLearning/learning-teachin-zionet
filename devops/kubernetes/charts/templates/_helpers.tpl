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

{{- define "app.dapr.annotations" -}}
{{- if .enabled }}
dapr.io/enabled: "true"
dapr.io/app-id: "{{ .appId }}"
dapr.io/app-port: "{{ .appPort }}"
{{- end }}
{{- end -}}
