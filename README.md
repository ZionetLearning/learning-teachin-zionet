# Backend – Local Configuration (Engine)

## Purpose

The **Engine** requires an Azure OpenAI API key to work correctly. Sensitive values must **never** be committed to the repository.

---

## Setup Instructions

If you want to run the project and use Azure OpenAI:

1. Copy the example configuration:
   ```bash
   cp ContainerApp/Engine/appsettings.Local.example.json ContainerApp/Engine/appsettings.Local.json
