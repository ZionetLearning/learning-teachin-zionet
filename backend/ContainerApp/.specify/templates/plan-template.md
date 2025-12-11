# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [.NET 10 (TeachIn backend) or NEEDS CLARIFICATION]  
**Primary Dependencies**: [Dapr, Azure Service Bus/queues, EF Core, SignalR or NEEDS CLARIFICATION]  
**Storage**: [PostgreSQL via Accessor service or N/A]  
**Testing**: [xUnit integration tests + unit tests or NEEDS CLARIFICATION]  
**Target Platform**: [Containerized services via Docker/Dapr or NEEDS CLARIFICATION]
**Project Type**: [Manager/Engine/Accessor backend feature - align with service boundaries]

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

- Service ownership: Does this feature clearly belong to Manager, Engine, Accessor, or a combination with explicit responsibilities?
- Communication: Are all cross-service calls using Dapr (sync) or Azure Service Bus queues/bindings (async/callbacks), with no direct DB or ad-hoc HTTP between services?
- Contracts & DTOs: Are request/response types named per conventions (`ActionNameRequest/Response`, `ActionNameAccessorRequest/Response`, etc.) and mapped at each boundary (no EF entities or shared internal models crossing layers)?
- Testing & Logging: Does the plan include tests (unit/integration) and structured logging for critical flows (auth, role checks, async work, SignalR notifications)?

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. For TeachIn we only have backend services; update the
  Manager/Engine/Accessor areas that this feature touches.
-->

```text
ContainerApp/
├── Manager/
│   ├── Endpoints/
│   ├── Services/
│   ├── Models/
│   └── ...
├── Engine/
│   ├── Endpoints/
│   ├── Services/
│   ├── Models/
│   └── ...
├── Accessor/
│   ├── Endpoints/
│   ├── Services/
│   ├── DB/
│   └── ...
└── UnitTests/
    ├── ManagerUnitTests/
    ├── EngineUnitTests/
    └── AccessorUnitTests/
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation                  | Why Needed         | Simpler Alternative Rejected Because |
| -------------------------- | ------------------ | ------------------------------------ |
| [e.g., 4th project]        | [current need]     | [why 3 projects insufficient]        |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient]  |
