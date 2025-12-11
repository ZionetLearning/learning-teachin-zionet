# Database Structure Implementation (Version 2) - Implementation Plan

This directory contains the complete implementation plan for implementing the new database structure in DatabaseAccessor (Version 2) to match the provided DBML schema.

**Important**: This is Version 2 - a completely new, independent implementation. All code will be created from scratch in DatabaseAccessor. The original Accessor service is used only as a reference for patterns and conventions - no code will be reused or migrated.

## Documents

- **[plan.md](./plan.md)** - Main implementation plan with technical context and project structure
- **[research.md](./research.md)** - Research findings and technical decisions
- **[data-model.md](./data-model.md)** - Complete entity definitions, relationships, and validation rules
- **[quickstart.md](./quickstart.md)** - Step-by-step implementation guide
- **[contracts/](./contracts/)** - API contracts (OpenAPI/YAML)
  - `users-api.yaml` - Users and UserDetails endpoints
  - `lessons-api.yaml` - Lessons, Tasks, Attendance, and Progress endpoints
  - `classes-api.yaml` - Classes endpoints with updated role structure

## Key Features

1. **Users & UserDetails**: Separate `Users` (auth) from `UserDetails` (profile) with 1:1 relationship
2. **Lessons & Tasks**: Structured `Lessons` with `LessonTasks` (no jsonb)
3. **Attendance Tracking**: `LessonAttendances` table for tracking student presence
4. **Progress Tracking**: `LessonTaskProgresses` table for tracking individual task completion
5. **Classes & Memberships**: `Classes` and `ClassMemberships` with integer role (0=Student, 1=Teacher)

## Implementation Order

1. Read `plan.md` for overview
2. Review `research.md` for technical decisions
3. Study `data-model.md` for entity structure
4. Follow `quickstart.md` for step-by-step implementation
5. Reference `contracts/` for API specifications

## Status

✅ Planning complete - Ready for implementation

## Implementation Approach

- **Version 2**: New implementation, independent from Accessor
- **From Scratch**: All models, services, configurations, endpoints created fresh
- **Reference Only**: Original Accessor used only for patterns/conventions
- **No Migration**: Fresh database structure - no data migration needed

