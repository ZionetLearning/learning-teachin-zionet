<!--
Sync Impact Report
- Version change: 0.0.0 → 1.0.0
- Modified principles: N/A (initial definition)
- Added sections: Core Principles; Architecture & Boundaries; Workflow & Quality; Governance
- Removed sections: Template placeholders only
- Templates updated:
	- ✅ .specify/templates/plan-template.md (Constitution Check placeholder)
	- ✅ .specify/templates/spec-template.md (no change needed, aligned)
	- ✅ .specify/templates/tasks-template.md (notes on tests vs. integration)
- Deferred TODOs:
	- TODO(RATIFICATION_DATE): Set to actual initial ratification date
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
include a written rationale and an explicit governance-approved
architecture update.

## Workflow & Quality

Feature work MUST follow a spec-first approach using SpecKit:

- Each feature MUST have a spec in `specs/[###-feature-name]/spec.md`
	with user scenarios for Students, Teachers, or Admins and measurable
	success criteria.
- Implementation plans (`plan.md`) MUST pass the Constitution Check by
	confirming service ownership, DTO naming, and communication patterns.
- Tasks (`tasks.md`) SHOULD group work by user story and include tests
	for critical paths (auth, role checks, async flows) before endpoint
	implementation.

Code reviews MUST verify:

- Service boundaries and communication rules are respected.
- DTO and mapping conventions are followed.
- Tests and logging are adequate for the change’s risk level.

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
- Versioning: Constitution changes follow semantic versioning
	(`MAJOR.MINOR.PATCH`), where MAJOR indicates breaking governance
	changes, MINOR adds or strengthens principles, and PATCH refines
	wording without changing intent.
- Compliance: SpecKit commands and review checklists MUST treat the
	"Constitution Check" as a blocking gate for new features. Violations
	MUST be explicitly documented and justified, not silently ignored.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): initial ratification date to be defined | **Last Amended**: 2025-12-08
