# Backend – Local Configuration (Engine)

## Purpose

The **Engine** requires an Azure OpenAI API key to work correctly. Sensitive values must **never** be committed to the repository.
The **Manager** requires an Jwt Secret. Sensitive values must **never** be committed to the repository.

---

## Setup Instructions

If you want to run the project:

1. Copy the example configuration:
   ```bash
   cp ContainerApp/Engine/appsettings.Local.example.json ContainerApp/Engine/appsettings.Local.json
   cp ContainerApp/Manager/appsettings.Local.example.json ContainerApp/Manager/appsettings.Local.json
