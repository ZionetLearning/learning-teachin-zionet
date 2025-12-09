<!--
Sync Impact Report
- Version change: 1.0.0 → 1.1.0
- Modified principles: N/A
- Added sections: Principle VI (Database Schema Management with EF Core Migrations)
- Removed sections: N/A
- Templates updated:
	- ✅ .specify/templates/plan-template.md (Constitution Check should verify migration requirement)
	- ✅ .specify/templates/spec-template.md (no change needed, aligned)
	- ✅ .specify/templates/tasks-template.md (should include migration tasks for DB changes)
- Follow-up TODOs: None
-->

# TeachIn Backend Constitution

## Core Principles

### I. Clear Service Boundaries (Manager, Engine, Accessor)

All backend code MUST respect the three-service architecture:
Manager for HTTP/API, orchestration, auth and SignalR; Engine for
long-running and AI workloads; Accessor as the single data and external
system boundary (PostgreSQL via EF Core and integrations). Cross-service
logic MUST flow through the appropriate service client interfaces and NOT
duplicate domain rules across services.

### II. Explicit Async Communication (Dapr + Queues)

All inter-service communication MUST use approved mechanisms: Dapr
service invocation for sync calls and Azure Service Bus queues (via Dapr
bindings/DotQueue) for async and callback patterns. Direct HTTP calls
between services or DB access outside Accessor are NOT allowed.
Callback paths from Accessor or Engine to Manager MUST go through
dedicated queues and SignalR notifications.

### III. Minimal APIs with Strong Contracts

Public HTTP endpoints in Manager MUST use Minimal API patterns with
grouped route registration, static handler methods, DI via parameters,
and consistent tags. Every endpoint MUST use dedicated DTOs following
the naming rules (e.g., `ActionNameRequest`, `ActionNameResponse`,
`ActionNameAccessorRequest`, `ActionNameEngineResponse`) and MUST NOT
expose EF entities or internal models directly. DTO models MUST be
declared as `sealed record` types with `init`-only properties by
default; mutable `set;` accessors are only allowed when there is a
specific need.

### IV. Mapping and Separation of Models (NON-NEGOTIABLE)

Data flowing between layers or services MUST always be mapped between
DTOs and models (Manager API DTOs ↔ Manager internal models ↔ Accessor
DTOs ↔ Accessor persistence models ↔ Engine DTOs). The same object MUST
NOT be passed through multiple layers unchanged. Mapping can be manual
or via profiles, but MUST be explicit and tested for key flows.

### V. Testing Discipline and Observability

IntrgrationTests are in a different project and not part of this scope.
for very complicated code we have UnitTest
Logging MUST use structured `ILogger` with clear messages and identifiers
(user, role, ids) to support debugging of async workflows.
Breaking changes to contracts MUST be covered by updated tests before merge.

### VI. Database Schema Management with EF Core Migrations

Any change to database tables or EF Core models in the Accessor service
MUST be accompanied by an EF Core migration. This includes adding,
removing, or modifying entities, properties, relationships, constraints,
or indexes. Migrations MUST be generated using `dotnet ef migrations add`
with a descriptive name reflecting the schema change. Direct database
schema modifications or deployment without migrations are NOT allowed.
Migration scripts MUST be reviewed for correctness and tested in a
non-production environment before merge.

## Architecture & Boundaries

The backend MUST remain a .NET 9, Dapr-based, three-service system for
the learning platform, serving Students, Teachers, and Admins
with clearly scoped access.

- Manager is the only entrypoint for frontend clients and SignalR
	connections; it owns authentication, authorization, and role-based
	access enforcement. and main logic point.
- Engine handles compute-heavy and AI-related processing and MUST NOT
	access the database directly.
- Accessor owns all data access (PostgreSQL via EF Core) and external
	systems (e.g., email) and MUST expose operations only via its public
	contracts.
- New capabilities MUST first ask: "Which service owns this concern?"
	and place code accordingly.

Any proposal to introduce a new service or bypass these boundaries MUST
include a written rationale and an explicit governance-approved architecture update.

## Workflow & Quality
Code reviews MUST verify:

- Service boundaries and communication rules are respected.
- DTO and mapping conventions are followed.
- Database changes include corresponding EF Core migrations.

Changes that break public contracts (Manager HTTP endpoints, Accessor /
Engine DTOs, queue message shapes) MUST document migration impact in
the PR description and update affected tests.d communication patterns.
- Tasks (`tasks.md`) SHOULD group work by user story
	for critical paths (auth, role checks, async flows) before endpoint implementation.

Code reviews MUST verify:

- Service boundaries and communication rules are respected.
- DTO and mapping conventions are followed.

Changes that break public contracts (Manager HTTP endpoints, Accessor /
Engine DTOs, queue message shapes) MUST document migration impact in
the PR description and update affected tests.

## Governance

This constitution defines non-negotiable technical and architectural
rules for the TeachIn backend. All contributors and reviewers are
responsible for enforcing it.

- Amendments: Any change to principles, architecture boundaries, or
	workflow requirements MUST be proposed via PR updating this file and
	explaining the rationale and impact.
- Compliance: SpecKit commands and review checklists MUST treat the
	"Constitution Check" as a blocking gate for new features. Violations
	MUST be explicitly documented and justified, not silently ignored.
**Version**: 1.1.0 | **Ratified**: TODO(RATIFICATION_DATE): initial ratification date to be defined | **Last Amended**: 2025-12-09
**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): initial ratification date to be defined | **Last Amended**: 2025-12-08
