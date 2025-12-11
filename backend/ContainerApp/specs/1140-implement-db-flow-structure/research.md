# Research: Database Structure Implementation (Version 2)

## Overview

This document consolidates research findings and decisions for implementing the new database structure in DatabaseAccessor (Version 2) as specified in the DBML schema. This is a completely new implementation - all code will be created from scratch. The original Accessor service is used only as a reference for implementation patterns.

## Research Areas

### 1. User/UserDetails Split Strategy

**Decision**: Implement 1:1 relationship with UserDetails.UserId as both primary key and foreign key to Users.UserId

**Rationale**: 
- Separates authentication concerns (Users) from profile data (UserDetails)
- Allows independent updates to profile without touching auth table
- Maintains referential integrity with cascade delete
- Follows PostgreSQL best practices for 1:1 relationships

**Alternatives considered**:
- Keep single Users table: Rejected - violates separation of concerns, makes auth table heavier
- Separate tables with nullable FK: Rejected - adds complexity, 1:1 should be enforced

**Implementation Notes**:
- UserDetails.UserId will be configured as both PK and FK
- Cascade delete ensures UserDetails removed when User deleted
- EF Core configuration: `HasOne().WithOne().HasForeignKey().OnDelete(DeleteBehavior.Cascade)`

### 2. Lesson Structure Migration

**Decision**: Replace ContentSections (jsonb) with structured LessonTasks table

**Rationale**:
- Structured data enables better querying and indexing
- Supports ordering via OrderIndex
- Allows individual task tracking and progress
- Better for reporting and analytics
- Aligns with relational database best practices

**Alternatives considered**:
- Keep ContentSections jsonb: Rejected - limits queryability and task-level operations
- Hybrid approach: Rejected - adds complexity, full migration is cleaner

**Implementation Strategy**:
- Create LessonTasks table from scratch
- Create Lesson model with ClassId, Status, ScheduledAt, etc. (no ContentSections)
- Create all endpoints/services to use LessonTasks from the start

**Implementation Notes**:
- LessonTasks will have OrderIndex for sequencing
- TaskType enum: warmup, exercise, game, homework, quiz, etc.
- IsRequired flag for optional vs mandatory tasks

### 3. ClassMembership Role Type

**Decision**: Change Role from enum to integer (0=Student, 1=Teacher)

**Rationale**:
- DBML spec explicitly requires integer
- Allows for future role expansion without enum changes
- More flexible for database-level queries
- Matches common pattern in relational schemas

**Alternatives considered**:
- Keep enum: Rejected - doesn't match spec, less flexible
- String: Rejected - less efficient, enum already exists in code

**Implementation Notes**:
- Update ClassMembership.Role to int
- Add conversion logic in mappers/services
- Update all queries to use integer comparison
- Maintain Role enum in C# code for type safety, convert at boundaries

### 4. Attendance Status Enum

**Decision**: Use integer status codes: 0=Absent, 1=Present, 2=Late, 3=Excused

**Rationale**:
- Matches DBML specification
- Allows efficient indexing and querying
- Standard pattern for status tracking

**Implementation Notes**:
- Create C# enum for type safety
- Store as int in database
- Index on (LessonId, Status) for attendance queries

### 5. Progress Tracking Status

**Decision**: Use integer status codes: 0=NotStarted, 1=InProgress, 2=Completed, 3=Skipped

**Rationale**:
- Matches DBML specification
- Clear state machine for task progress
- ProgressPercent (0-100) provides granular tracking

**Implementation Notes**:
- Create C# enum for type safety
- Store as int in database
- Index on (UserId, Status) for progress queries
- LastUpdatedAt for tracking changes

### 6. Lesson Status Enum

**Decision**: Use integer status codes: 0=Planned, 1=InProgress, 2=Completed, 3=Cancelled

**Rationale**:
- Matches DBML specification
- Supports lesson lifecycle management
- Enables filtering by status

**Implementation Notes**:
- Add Status field to Lesson model
- Default to 0 (Planned)
- Index on Status for filtering

### 7. Initial Migration Strategy

**Decision**: Create EF Core initial migration for new schema

**Rationale**:
- EF Core migrations provide version control
- Clean initial migration for new database structure
- Rollback support via Down() method
- Standard approach for new implementations

**Alternatives considered**:
- Manual SQL scripts: Rejected - loses EF Core integration, harder to maintain
- Separate migration tool: Rejected - adds complexity, EF Core sufficient

**Implementation Notes**:
- Initial migration will create all tables from scratch:
  1. Users table (core auth only)
  2. UserDetails table (profile data)
  3. Classes table
  4. ClassMemberships table (with integer role)
  5. Lessons table (with ClassId, Status, ScheduledAt, etc.)
  6. LessonTasks table
  7. LessonAttendances table
  8. LessonTaskProgresses table
- No data migration needed - this is a fresh start

### 8. Independence from Accessor

**Decision**: Complete independence - no code reuse from Accessor

**Rationale**:
- This is Version 2 - a new implementation
- Allows for clean architecture without legacy constraints
- Original Accessor serves only as a reference for patterns

**Implementation Notes**:
- All models created from scratch in DatabaseAccessor
- All services created from scratch
- All configurations created from scratch
- Use Accessor only as a reference for:
  - Naming conventions
  - Code organization patterns
  - EF Core configuration patterns
  - Endpoint structure patterns

### 9. Indexing Strategy

**Decision**: Implement indexes as specified in DBML

**Rationale**:
- Optimizes common query patterns
- Supports efficient joins and filters
- Matches DBML specification

**Indexes to create**:
- Users: Email (unique), Role, AcsUserId
- UserDetails: UserId (unique)
- Classes: Name (unique)
- ClassMemberships: (ClassId, UserId, Role) PK, (UserId, Role, ClassId), (ClassId, Role, UserId)
- Lessons: ClassId, (ClassId, ScheduledAt), CreatedBy
- LessonTasks: LessonId, (LessonId, OrderIndex)
- LessonAttendances: (LessonId, UserId) PK, (UserId, LessonId), (LessonId, Status)
- LessonTaskProgresses: (LessonTaskId, UserId) PK, (UserId, LessonTaskId), (UserId, Status)

## Technology Choices

### Entity Framework Core 9
- **Why**: Already in use, supports PostgreSQL, migrations, and complex relationships
- **Configuration**: Use IEntityTypeConfiguration<T> pattern (already established)

### PostgreSQL jsonb
- **Why**: Already in use for Interests field in UserDetails
- **Usage**: Continue using for Interests array, remove from Lessons

### Enum Conversions
- **Why**: Type safety in C#, flexibility in database
- **Pattern**: Store as string/int in DB, convert at EF Core level using HasConversion

## Open Questions Resolved

1. ✅ **Q**: Should UserDetails be optional or required?
   **A**: Required - every User must have UserDetails (1:1 relationship)

2. ✅ **Q**: How to handle existing Lessons with ContentSections?
   **A**: Migrate each ContentSection to a LessonTask with OrderIndex based on array position

3. ✅ **Q**: Should LessonTasks be deletable independently?
   **A**: Yes, but cascade delete when Lesson is deleted (as per DBML)

4. ✅ **Q**: How to handle ClassMembership role migration?
   **A**: Convert enum to int: Student=0, Teacher=1, Admin=2 (but Admin not in ClassMemberships per spec)

5. ✅ **Q**: Should we maintain old endpoints during migration?
   **A**: No - update endpoints to use new structure, but keep data migration safe

## References

- [EF Core Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL Indexes](https://www.postgresql.org/docs/current/indexes.html)
- DBML Schema provided in user input

