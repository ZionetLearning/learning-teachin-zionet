# VS Code Debugging for .NET Services Running in Containers

This guide describes how to configure VS Code so you can attach the .NET debugger to containerized services built with Docker Compose. It works whether you are debugging a single worker or several backend services while other containers (gateways, proxies, infrastructure) continue to run normally.

---

## 1. Prerequisites

- **VS Code extensions**:  
  - C# Dev Kit (or the legacy C# extension) for the coreclr debugger  
  - Docker extension (optional but useful for inspecting containers)  
- **Docker Desktop** (or compatible Docker Engine) running locally.  
- **.NET SDK** version that matches the base images you plan to use (for example, .NET 8 or 9).  
- Required **environment variables** for your Compose stack (database passwords, API keys, connection strings, etc.) exported in your shell or stored in an `.env`.

> Tip: Verify Docker access with `docker compose version` before launching VS Code tasks.

---

## 2. Project Layout Expectations

Every project will differ, but the repo root (VS Code’s `${workspaceFolder}`) generally needs:

```
MyApp/
├─ ServiceA/
│   ├─ Dockerfile
│   └─ Dockerfile.debug
├─ ServiceB/
│   ├─ Dockerfile
│   └─ Dockerfile.debug
├─ ServiceC/
│   ├─ Dockerfile
│   └─ Dockerfile.debug
├─ docker-compose.yml
├─ docker-compose.debug.yml
└─ .vscode/
    ├─ launch.json
    └─ tasks.json
```

Only the services that need debugger support require `Dockerfile.debug` entries and launch configurations. Others can continue to run from their standard Dockerfiles.

---

## 3. Debug-Friendly Dockerfiles

For every service you want to debug, create a `Dockerfile.debug` alongside the existing runtime Dockerfile. That debug Dockerfile should:

1. **Installs vsdbg** into `/vsdbg` inside the runtime image:  
   ```dockerfile
   RUN apt-get update && apt-get install -y curl unzip && rm -rf /var/lib/apt/lists/*
   RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg
   ```
2. **Builds in Debug** mode so PDB files are emitted:  
   ```dockerfile
   RUN dotnet publish -c Debug -o /app/publish \
       /p:DebugType=portable /p:DebugSymbols=true /p:EmbedAllSources=false
   ```
3. Keep the final stage lean by copying the publish output and setting useful env vars:  
   ```dockerfile
   ENV DOTNET_USE_POLLING_FILE_WATCHER=true
   ENTRYPOINT ["dotnet", "ServiceName.dll"]
   ```

All build stages should set `WORKDIR /src` (or `/src/<ServiceName>`) so the resulting PDBs reference `/src/...`. This path will be mapped back to your host sources via `sourceFileMap`.

---

## 4. Compose Override for Debugging

Retain `docker-compose.yml` for the normal runtime stack. Then add a `docker-compose.debug.yml` that overrides only the services you plan to debug. Example (rename services to match your project):

```yaml
services:
  service-a:
    build:
      context: ./ServiceA
      dockerfile: Dockerfile.debug
    container_name: service-a-debug
    volumes:
      - ./ServiceA:/src/ServiceA:ro

  service-b:
    build:
      context: ./ServiceB
      dockerfile: Dockerfile.debug
    container_name: service-b-debug
    ports:
      - 6001:6001
    volumes:
      - ./ServiceB:/src/ServiceB:ro
```

Key points:
- **Bind mount** each service folder into `/src/<Service>` (read-only is fine). This keeps container binaries aligned with the host source tree, so breakpoints bind.
- **Unique container names** (e.g., `*-debug`) make attaching predictable.
- Services you are *not* debugging (API gateways, background agents, infrastructure) simply inherit their settings from the base compose file with no overrides necessary.

---

## 5. VS Code Tasks

Add shell tasks in `.vscode/tasks.json` to coordinate Compose operations. Example:

```jsonc
{
  "label": "docker-compose-debug-up",
  "type": "shell",
  "command": "docker",
  "args": [
    "compose",
    "-f", "docker-compose.yml",
    "-f", "docker-compose.debug.yml",
    "up", "-d", "--build"
  ],
  "options": { "cwd": "${workspaceFolder}" }
}
```

Additional helpful tasks:
- `docker-compose-debug-down`: stops/removes containers (`down --remove-orphans -v`).
- `docker-compose-debug-logs`: tails container logs for quick diagnostics.
- `docker-compose-debug-rebuild`: forces a rebuild/recreate when dependencies changed.

Tasks centralize the exact Compose command so every developer runs the same thing via VS Code instead of terminal scripts.

---

## 6. VS Code Launch Configurations

Create one `coreclr` attach configuration per debuggable service. Replace names and paths with your service identifiers:

```jsonc
{
  "name": "Docker: Attach to Service A",
  "type": "coreclr",
  "request": "attach",
  "processName": "dotnet",
  "pipeTransport": {
    "pipeProgram": "docker",
    "pipeArgs": ["exec", "-i", "service-a-debug"],
    "debuggerPath": "/vsdbg/vsdbg",
    "pipeCwd": "${workspaceFolder}",
    "quoteArgs": false
  },
  "sourceFileMap": { "/src": "${workspaceFolder}/ServiceA" },
  "postDebugTask": "docker-compose-debug-down",
  "presentation": { "group": "Docker Services", "order": 1 }
}
```

Make sure:
- `pipeArgs` targets the container name from your override file.
- `sourceFileMap` maps `/src` (inside the container) back to the host folder that contains the project.
- `postDebugTask` (optional) tears down containers after the session.

For multi-service debugging, define a compound configuration referencing whichever services you want to attach to:

```jsonc
{
  "name": "Debug All Docker Services",
  "configurations": [
    "Docker: Attach to Service A",
    "Docker: Attach to Service B",
    "Docker: Attach to Service C"
  ],
  "stopAll": true,
  "preLaunchTask": "docker-compose-debug-up"
}
```

Omit services you do not want to debug—only include those needing breakpoints.

---

## 7. Debugging Workflow

1. **Launch compound (or single) configuration** from VS Code. The `preLaunchTask` builds and starts containers with the debug overrides.
2. **Wait for startup**: open the “Tasks” panel to ensure `docker-compose` finished pulling/building and all containers are healthy.
3. **Attach**: VS Code automatically attaches to each `dotnet` process named in launch configurations via `docker exec`. Breakpoints will stay gray until the relevant assembly loads.
4. **Interact with services** (HTTP requests, queue messages, gRPC calls, etc.) to hit breakpoints.
5. **Stop**: hitting Stop in VS Code ends all sessions; use `docker-compose-debug-down` to clean up or let `postDebugTask` do it automatically.

> If breakpoints never bind, confirm the source map path matches the mount path and that the container’s DLL names align with your project outputs.

---

## 8. Handling Optional / Non-Debug Services

- Infrastructure services (databases, caches, Dapr placement, emulators, queues) stay defined only in `docker-compose.yml`; they do not require debugger configuration.
- Runtime-only services (API gateways, BFFs, background processors) can keep their Release Dockerfiles. They will run alongside debug-enabled services when you execute `docker-compose-debug-up` but will not have `vsdbg` installed or VS Code launch entries—perfect for scenarios where you just need them reachable for integration calls.
- If later you need to debug one of these services, add a `Dockerfile.debug`, an override entry, and a launch config following the same pattern.

---

## 9. Extending to New Projects

When applying this setup elsewhere:

1. **Copy or recreate the `.vscode` folder** and adjust container names, service folders, and port numbers.
2. **Add `Dockerfile.debug` files** for each service that needs debugging; keep the vsdbg path and Debug publish settings identical.
3. **Create/merge a `docker-compose.debug.yml`** that layers on top of the project’s default compose file. Only override services requiring debugging.
4. **Verify environment variables** and network bindings match the new project (databases, message brokers, external APIs, etc.).

Following these steps ensures every developer (or automation agent) can launch VS Code, run the “Debug All Docker Services” compound, and start debugging any containerized .NET service without touching the global Docker or VS Code settings.

---

Happy debugging!
