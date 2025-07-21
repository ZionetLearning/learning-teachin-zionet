# Component Tests README

> **Location:** `backend/ComponentTests/README.md`

This document explains how to set up and run the component tests locally, including the dummy AI integration test using OpenAI.


## 🔑 Configure OpenAI Key via User Secrets

1. Change directory to the tests folder:
   ```bash
   cd backend/ComponentTests
**Initialize User Secrets (if you haven’t already):**
dotnet user-secrets init
**Store your OpenAI API key:**
dotnet user-secrets set "OpenAI:ApiKey" "sk-..."
The test runner will automatically pick up OPENAI_API_KEY from environment variables or User Secrets.

🚀 Run Tests Locally