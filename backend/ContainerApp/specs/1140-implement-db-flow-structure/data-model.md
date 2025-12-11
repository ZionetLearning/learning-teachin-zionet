# Data Model: Database Structure Refactor

## Overview

This document defines the entity models, relationships, and validation rules for the refactored database structure.

## Entities

### Users

**Table**: `Users`  
**Purpose**: Core authentication and user identification

**Fields**:
- `UserId` (Guid, PK): Unique identifier
- `Email` (string, 100, required, unique): User email address
- `Password` (string, 255, required): Hashed password
- `Role` (string, required): User role - "Admin", "Teacher", or "Student"
- `AcsUserId` (string, 255, nullable): Azure Communication Services user ID

**Indexes**:
- `Email` (unique)
- `Role`
- `AcsUserId`

**Validation Rules**:
- Email must be valid format
- Password must be hashed (never plain text)
- Role must be one of: Admin, Teacher, Student

**Relationships**:
- 1:1 with UserDetails (cascade delete)

---

### UserDetails

**Table**: `UserDetails`  
**Purpose**: Extended user profile information

**Fields**:
- `UserId` (Guid, PK, FK → Users.UserId): Same as Users.UserId
- `FirstName` (string, required): User's first name
- `LastName` (string, required): User's last name
- `PreferredLanguageCode` (string, required, default: "en"): Language preference - "en" or "he"
- `HebrewLevelValue` (string, nullable): Hebrew proficiency - "beginner", "intermediate", "advanced", or "fluent" (students only)
- `Interests` (jsonb, required, default: []): Array of interest strings
- `LearningStyle` (string, nullable): Learning style preference
- `PriorKnowledge` (int, nullable): Prior knowledge level
- `Profile` (string, nullable): User profile text
- `AvatarPath` (string, nullable): Path to avatar image
- `AvatarContentType` (string, nullable): MIME type of avatar
- `AvatarUpdatedAtUtc` (DateTime, nullable): Last avatar update timestamp

**Indexes**:
- `UserId` (unique)

**Validation Rules**:
- PreferredLanguageCode must be "en" or "he"
- HebrewLevelValue only applicable for students (Role = "Student")
- Interests must be valid JSON array

**Relationships**:
- 1:1 with Users (cascade delete)

---

### Classes

**Table**: `Classes`  
**Purpose**: Class/group management

**Fields**:
- `ClassId` (Guid, PK): Unique identifier
- `Name` (string, 100, required, unique): Class name
- `Code` (string, 50, nullable): Join code for students
- `Description` (string, nullable): Class description
- `CreatedAt` (DateTime, required, default: NOW()): Creation timestamp

**Indexes**:
- `Name` (unique)

**Validation Rules**:
- Name must be unique
- Code must be unique if provided

**Relationships**:
- 1:Many with ClassMemberships (cascade delete)
- 1:Many with Lessons (cascade delete)

---

### ClassMemberships

**Table**: `ClassMemberships`  
**Purpose**: Many-to-many relationship between Users and Classes with role

**Fields**:
- `ClassId` (Guid, PK, FK → Classes.ClassId): Class identifier
- `UserId` (Guid, PK, FK → Users.UserId): User identifier
- `Role` (int, PK, required): Role in class - 0 = Student, 1 = Teacher
- `AddedBy` (Guid, required, FK → Users.UserId): User who added this member
- `AddedAt` (DateTime, required): Timestamp when member was added

**Indexes**:
- `(ClassId, UserId, Role)` (composite PK)
- `(UserId, Role, ClassId)`
- `(ClassId, Role, UserId)`

**Validation Rules**:
- Role must be 0 (Student) or 1 (Teacher)
- User cannot be added twice with same role
- User can have different roles in different classes

**Relationships**:
- Many:1 with Classes (cascade delete)
- Many:1 with Users (cascade delete)
- Many:1 with Users (AddedBy, cascade delete)

---

### Lessons

**Table**: `Lessons`  
**Purpose**: Lesson/class session management

**Fields**:
- `LessonId` (Guid, PK): Unique identifier
- `ClassId` (Guid, required, FK → Classes.ClassId): Class this lesson belongs to
- `Title` (string, 150, required): Lesson title
- `Description` (string, nullable): Lesson description
- `ScheduledAt` (DateTime, required): Planned start time
- `DurationMinutes` (int, nullable): Planned duration in minutes
- `Status` (int, required, default: 0): Lesson status - 0 = Planned, 1 = InProgress, 2 = Completed, 3 = Cancelled
- `CreatedBy` (Guid, required, FK → Users.UserId): User who created the lesson
- `CreatedAt` (DateTime, required, default: NOW()): Creation timestamp
- `UpdatedAt` (DateTime, nullable): Last update timestamp

**Indexes**:
- `ClassId`
- `(ClassId, ScheduledAt)`
- `CreatedBy`

**Validation Rules**:
- Status must be 0-3
- ScheduledAt must be in future for Planned lessons
- DurationMinutes must be positive if provided

**Relationships**:
- Many:1 with Classes (cascade delete)
- Many:1 with Users (CreatedBy)
- 1:Many with LessonTasks (cascade delete)
- 1:Many with LessonAttendances (cascade delete)

---

### LessonTasks

**Table**: `LessonTasks`  
**Purpose**: Individual tasks within a lesson

**Fields**:
- `LessonTaskId` (Guid, PK): Unique identifier
- `LessonId` (Guid, required, FK → Lessons.LessonId): Parent lesson
- `OrderIndex` (int, required): Order within the lesson (0-based)
- `Title` (string, 150, required): Task title
- `Description` (string, nullable): Task description
- `TaskType` (string, nullable): Task type - "warmup", "exercise", "game", "homework", "quiz", etc.
- `ExpectedDurationMinutes` (int, nullable): Expected duration in minutes
- `IsRequired` (bool, required, default: true): Whether task is mandatory

**Indexes**:
- `LessonId`
- `(LessonId, OrderIndex)`

**Validation Rules**:
- OrderIndex must be unique within LessonId
- OrderIndex must be >= 0
- ExpectedDurationMinutes must be positive if provided

**Relationships**:
- Many:1 with Lessons (cascade delete)
- 1:Many with LessonTaskProgresses (cascade delete)

---

### LessonAttendances

**Table**: `LessonAttendances`  
**Purpose**: Track student attendance for lessons

**Fields**:
- `LessonId` (Guid, PK, FK → Lessons.LessonId): Lesson identifier
- `UserId` (Guid, PK, FK → Users.UserId): User identifier
- `Status` (int, required, default: 0): Attendance status - 0 = Absent, 1 = Present, 2 = Late, 3 = Excused
- `JoinedAt` (DateTime, nullable): When user joined the lesson
- `LeftAt` (DateTime, nullable): When user left the lesson
- `Comment` (string, nullable): Additional notes

**Indexes**:
- `(LessonId, UserId)` (composite PK)
- `(UserId, LessonId)`
- `(LessonId, Status)`

**Validation Rules**:
- Status must be 0-3
- LeftAt must be after JoinedAt if both provided
- One record per (LessonId, UserId)

**Relationships**:
- Many:1 with Lessons (cascade delete)
- Many:1 with Users (cascade delete)

---

### LessonTaskProgresses

**Table**: `LessonTaskProgresses`  
**Purpose**: Track individual student progress on lesson tasks

**Fields**:
- `LessonTaskId` (Guid, PK, FK → LessonTasks.LessonTaskId): Task identifier
- `UserId` (Guid, PK, FK → Users.UserId): User identifier
- `Status` (int, required, default: 0): Progress status - 0 = NotStarted, 1 = InProgress, 2 = Completed, 3 = Skipped
- `ProgressPercent` (int, required, default: 0): Completion percentage (0-100)
- `Score` (int, nullable): Task score if applicable
- `LastUpdatedAt` (DateTime, required, default: NOW()): Last update timestamp
- `Comment` (string, nullable): Additional notes

**Indexes**:
- `(LessonTaskId, UserId)` (composite PK)
- `(UserId, LessonTaskId)`
- `(UserId, Status)`

**Validation Rules**:
- Status must be 0-3
- ProgressPercent must be 0-100
- Score must be positive if provided
- One record per (LessonTaskId, UserId)

**Relationships**:
- Many:1 with LessonTasks (cascade delete)
- Many:1 with Users (cascade delete)

---

## Relationships Summary

```
Users (1) ────< (1) UserDetails
Users (1) ────< (Many) ClassMemberships ────> (Many) Classes
Users (1) ────< (Many) Lessons (CreatedBy)
Users (1) ────< (Many) LessonAttendances
Users (1) ────< (Many) LessonTaskProgresses

Classes (1) ────< (Many) Lessons
Lessons (1) ────< (Many) LessonTasks
Lessons (1) ────< (Many) LessonAttendances
LessonTasks (1) ────< (Many) LessonTaskProgresses
```

## State Transitions

### Lesson Status
```
Planned (0) → InProgress (1) → Completed (2)
Planned (0) → Cancelled (3)
InProgress (1) → Completed (2)
InProgress (1) → Cancelled (3)
```

### LessonTaskProgress Status
```
NotStarted (0) → InProgress (1) → Completed (2)
NotStarted (0) → Skipped (3)
InProgress (1) → Completed (2)
InProgress (1) → Skipped (3)
```

### LessonAttendance Status
```
Absent (0) → Present (1)
Absent (0) → Late (2)
Absent (0) → Excused (3)
```

## Enums

### Role (Users table)
```csharp
public enum Role
{
    Student = 0,
    Teacher = 1,
    Admin = 2
}
```

### SupportedLanguage (UserDetails)
```csharp
public enum SupportedLanguage
{
    en,
    he
}
```

### HebrewLevel (UserDetails)
```csharp
public enum HebrewLevel
{
    beginner,
    intermediate,
    advanced,
    fluent
}
```

### LessonStatus (Lessons)
```csharp
public enum LessonStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}
```

### AttendanceStatus (LessonAttendances)
```csharp
public enum AttendanceStatus
{
    Absent = 0,
    Present = 1,
    Late = 2,
    Excused = 3
}
```

### TaskProgressStatus (LessonTaskProgresses)
```csharp
public enum TaskProgressStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Skipped = 3
}
```

### ClassMembershipRole (ClassMemberships)
```csharp
// Stored as int in DB: 0 = Student, 1 = Teacher
// Note: Admin role not used in ClassMemberships
```

## Migration Notes

1. **Users → UserDetails**: Profile fields moved from Users to UserDetails
2. **Lessons.ContentSections → LessonTasks**: Each ContentSection becomes a LessonTask
3. **ClassMemberships.Role**: Convert from enum to int (0=Student, 1=Teacher)
4. **New tables**: LessonAttendances, LessonTaskProgresses
5. **Lessons updates**: Add ClassId, Status, ScheduledAt, DurationMinutes, UpdatedAt

