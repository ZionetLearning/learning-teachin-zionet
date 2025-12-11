# Implementation Plan: Database Structure Implementation (Version 2)

**Branch**: `1140-implement-db-flow-structure` | **Date**: 2025-01-27 | **Spec**: [DBML Schema]
**Input**: DBML schema defining Users/UserDetails split, Lessons with Tasks, Attendance, and Progress tracking

**Note**: This is **Version 2** - a completely new, independent implementation in DatabaseAccessor. All models, services, configurations, and endpoints will be created from scratch. The original Accessor service is used only as a reference/guideline for implementation patterns and conventions, but no code will be reused or migrated.

## Summary

Implement a new database structure in DatabaseAccessor (Version 2) to support:
1. **User Management**: Split `Users` table into `Users` (core auth) and `UserDetails` (profile data) with 1:1 relationship
2. **Lessons Enhancement**: Transform `Lessons` from ContentSections (jsonb) to structured `LessonTasks` with ordering
3. **Attendance Tracking**: Add `LessonAttendances` table for tracking student presence
4. **Progress Tracking**: Add `LessonTaskProgresses` table for tracking individual task completion
5. **Class Membership**: Update `ClassMemberships` to use integer role (0=Student, 1=Teacher) instead of enum

## Technical Context

**Language/Version**: .NET 10 (DatabaseAccessor project)  
**Primary Dependencies**: EF Core, PostgreSQL, Minimal APIs  
**Storage**: PostgreSQL via DatabaseAccessor service  
**Testing**: xUnit integration tests + unit tests  
**Target Platform**: Containerized services via Docker  
**Project Type**: DatabaseAccessor - new implementation (Version 2), independent from Accessor

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- ✅ **Service ownership**: This feature belongs to DatabaseAccessor service - new implementation (Version 2)
- ✅ **Communication**: DatabaseAccessor is independent - no dependencies on Accessor code
- ✅ **Contracts & DTOs**: All DTOs, models, and contracts will be created from scratch in DatabaseAccessor
- ✅ **Testing & Logging**: Plan includes unit tests for all new entities and structured logging for all operations

## Project Structure

### Documentation (this feature)

```text
specs/1140-implement-db-flow-structure/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
ContainerApp/
├── Accessors/
│   └── DatabaseAccessor/                    # Version 2 - NEW implementation
│       ├── DB/
│       │   ├── DatabaseAccessorDbContext.cs  # NEW - create from scratch
│       │   ├── Configurations/               # NEW - all configurations created fresh
│       │   │   ├── UsersConfiguration.cs
│       │   │   ├── UserDetailsConfiguration.cs
│       │   │   ├── ClassesConfiguration.cs
│       │   │   ├── ClassMembershipsConfiguration.cs
│       │   │   ├── LessonsConfiguration.cs
│       │   │   ├── LessonTasksConfiguration.cs
│       │   │   ├── LessonAttendancesConfiguration.cs
│       │   │   └── LessonTaskProgressesConfiguration.cs
│       │   └── Migrations/
│       │       └── [Initial migration]
│       ├── Models/                          # NEW - all models created from scratch
│       │   ├── Users/
│       │   │   ├── User.cs
│       │   │   └── UserDetails.cs
│       │   ├── Classes/
│       │   │   ├── Class.cs
│       │   │   └── ClassMembership.cs
│       │   └── Lessons/
│       │       ├── Lesson.cs
│       │       ├── LessonTask.cs
│       │       ├── LessonAttendance.cs
│       │       └── LessonTaskProgress.cs
│       ├── Services/                         # NEW - all services created from scratch
│       │   ├── Interfaces/
│       │   │   ├── IUserService.cs
│       │   │   ├── IClassService.cs
│       │   │   └── ILessonService.cs
│       │   └── Implementations/
│       │       ├── UserService.cs
│       │       ├── ClassService.cs
│       │       └── LessonService.cs
│       ├── Repositories/                     # NEW - repository pattern
│       │   ├── Interfaces/
│       │   └── Implementations/
│       ├── Mapping/                          # NEW - all mappers created from scratch
│       │   ├── UsersMapper.cs
│       │   ├── ClassesMapper.cs
│       │   └── LessonsMapper.cs
│       ├── Endpoints/                        # NEW - all endpoints created from scratch
│       │   ├── UsersEndpoints.cs
│       │   ├── ClassesEndpoints.cs
│       │   └── LessonsEndpoints.cs
│       └── Program.cs                        # Update to register new services
└── UnitTests/                                # NEW - test project for DatabaseAccessor
    └── DatabaseAccessorUnitTests/
        ├── UsersTests.cs
        ├── ClassesTests.cs
        ├── LessonsTests.cs
        └── IntegrationTests.cs
```

**Structure Decision**: This is a completely new implementation (Version 2) in DatabaseAccessor. All code is created from scratch. Original Accessor is used only as a reference for patterns and conventions. No code reuse or migration from Accessor.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | No violations | DatabaseAccessor is independent - new implementation from scratch |

