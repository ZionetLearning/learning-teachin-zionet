# Quickstart: Database Structure Implementation (Version 2)

## Overview

This guide provides step-by-step instructions for implementing the new database structure in DatabaseAccessor (Version 2) as defined in the DBML schema. **All code will be created from scratch** - this is a new implementation, not a refactor. The original Accessor service is used only as a reference for patterns and conventions.

## Prerequisites

- .NET 10 SDK installed
- PostgreSQL database accessible
- EF Core tools installed: `dotnet tool install --global dotnet-ef`
- Access to DatabaseAccessor project: `backend/ContainerApp/Accessors/DatabaseAccessor`
- Reference to original Accessor (for patterns only): `backend/ContainerApp/Accessor`

## Implementation Steps

### Phase 1: Create Models from Scratch

#### 1.1 Create User Model

**File**: `Accessors/DatabaseAccessor/Models/Users/User.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DatabaseAccessor.Models.Users;

[Table("Users")]
public class User
{
    [Key]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Email { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Password { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role Role { get; set; }

    [MaxLength(255)]
    public string? AcsUserId { get; set; }

    // Navigation property
    public UserDetails? UserDetails { get; set; }
}

public enum Role
{
    Student = 0,
    Teacher = 1,
    Admin = 2
}
```

#### 1.2 Create UserDetails Model

**File**: `Accessors/DatabaseAccessor/Models/Users/UserDetails.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DatabaseAccessor.Models.Users;

[Table("UserDetails")]
public class UserDetailsModel
{
    [Key]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SupportedLanguage PreferredLanguageCode { get; set; } = SupportedLanguage.en;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HebrewLevel? HebrewLevelValue { get; set; }

    [Required]
    public required List<string> Interests { get; set; } = [];

    public string? LearningStyle { get; set; }
    public int? PriorKnowledge { get; set; }
    public string? Profile { get; set; }
    public string? AvatarPath { get; set; }
    public string? AvatarContentType { get; set; }
    public DateTime? AvatarUpdatedAtUtc { get; set; }

    // Navigation property
    public UserModel User { get; set; } = null!;
}
```

#### 1.3 Create Class Model

**File**: `Accessors/DatabaseAccessor/Models/Classes/Class.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseAccessor.Models.Classes;

[Table("Classes")]
public class Class
{
    [Key]
    public Guid ClassId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ClassMembership> Memberships { get; set; } = [];
    public ICollection<Lesson> Lessons { get; set; } = [];
}
```

#### 1.4 Create ClassMembership Model

**File**: `Accessors/DatabaseAccessor/Models/Classes/ClassMembership.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccessor.Models.Users;

namespace DatabaseAccessor.Models.Classes;

[Table("ClassMemberships")]
public class ClassMembership
{
    [Key]
    [ForeignKey(nameof(Class))]
    public Guid ClassId { get; set; }

    [Key]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Key]
    [Required]
    public int Role { get; set; } // 0 = Student, 1 = Teacher

    [Required]
    [ForeignKey(nameof(AddedByUser))]
    public Guid AddedBy { get; set; }

    [Required]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Class Class { get; set; } = null!;
    public User User { get; set; } = null!;
    public User AddedByUser { get; set; } = null!;
}
```

#### 1.5 Create Lesson Model

**File**: `Accessors/DatabaseAccessor/Models/Lessons/Lesson.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccessor.Models.Classes;
using DatabaseAccessor.Models.Users;

namespace DatabaseAccessor.Models.Lessons;

[Table("Lessons")]
public class Lesson
{
    [Key]
    public Guid LessonId { get; set; }

    [Required]
    [ForeignKey(nameof(Class))]
    public Guid ClassId { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    public int? DurationMinutes { get; set; }

    [Required]
    public int Status { get; set; } = 0; // 0=Planned, 1=InProgress, 2=Completed, 3=Cancelled

    [Required]
    [ForeignKey(nameof(CreatedByUser))]
    public Guid CreatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Class Class { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<LessonTask> Tasks { get; set; } = [];
    public ICollection<LessonAttendance> Attendances { get; set; } = [];
}
```

#### 1.6 Create LessonTask Model

**File**: `Accessors/DatabaseAccessor/Models/Lessons/LessonTask.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseAccessor.Models.Lessons;

[Table("LessonTasks")]
public class LessonTask
{
    [Key]
    public Guid LessonTaskId { get; set; }

    [Required]
    [ForeignKey(nameof(Lesson))]
    public Guid LessonId { get; set; }

    [Required]
    public int OrderIndex { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Title { get; set; }

    public string? Description { get; set; }
    public string? TaskType { get; set; }
    public int? ExpectedDurationMinutes { get; set; }

    [Required]
    public bool IsRequired { get; set; } = true;

    // Navigation property
    public Lesson Lesson { get; set; } = null!;
}
```

#### 1.7 Create LessonAttendance Model

**File**: `Accessors/DatabaseAccessor/Models/Lessons/LessonAttendance.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccessor.Models.Users;

namespace DatabaseAccessor.Models.Lessons;

[Table("LessonAttendances")]
public class LessonAttendance
{
    [Key]
    [ForeignKey(nameof(Lesson))]
    public Guid LessonId { get; set; }

    [Key]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Required]
    public int Status { get; set; } = 0; // 0=Absent, 1=Present, 2=Late, 3=Excused

    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public string? Comment { get; set; }

    // Navigation properties
    public Lesson Lesson { get; set; } = null!;
    public UserModel User { get; set; } = null!;
}
```

#### 1.8 Create LessonTaskProgress Model

**File**: `Accessors/DatabaseAccessor/Models/Lessons/LessonTaskProgress.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccessor.Models.Users;

namespace DatabaseAccessor.Models.Lessons;

[Table("LessonTaskProgresses")]
public class LessonTaskProgress
{
    [Key]
    [ForeignKey(nameof(LessonTask))]
    public Guid LessonTaskId { get; set; }

    [Key]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Required]
    public int Status { get; set; } = 0; // 0=NotStarted, 1=InProgress, 2=Completed, 3=Skipped

    [Required]
    [Range(0, 100)]
    public int ProgressPercent { get; set; } = 0;

    public int? Score { get; set; }

    [Required]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    public string? Comment { get; set; }

    // Navigation properties
    public LessonTask LessonTask { get; set; } = null!;
    public UserModel User { get; set; } = null!;
}
```


### Phase 2: Create EF Core Configurations

#### 2.1 Create UserDetailsConfiguration

**File**: `Accessor/DB/Configurations/UserDetailsConfiguration.cs`

```csharp
using DatabaseAccessor.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class UserDetailsConfiguration : IEntityTypeConfiguration<UserDetails>
{
    public void Configure(EntityTypeBuilder<UserDetails> builder)
    {
        builder.ToTable("UserDetails");

        builder.HasKey(ud => ud.UserId);

        builder.Property(ud => ud.FirstName).IsRequired();
        builder.Property(ud => ud.LastName).IsRequired();

        builder.Property(ud => ud.PreferredLanguageCode)
            .HasConversion<string>()
            .HasDefaultValue(SupportedLanguage.en)
            .IsRequired();

        builder.Property(ud => ud.HebrewLevelValue)
            .HasConversion<string>()
            .IsRequired(false);

        builder.Property(ud => ud.Interests)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .IsRequired();

        builder.HasIndex(ud => ud.UserId).IsUnique();

        // 1:1 relationship with Users
        builder.HasOne(ud => ud.User)
            .WithOne(u => u.UserDetails)
            .HasForeignKey<UserDetails>(ud => ud.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### 2.3 Create ClassesConfiguration

**File**: `Accessors/DatabaseAccessor/DB/Configurations/ClassesConfiguration.cs`

```csharp
using DatabaseAccessor.Models.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseAccessor.DB.Configurations;

public class ClassesConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");

        builder.HasKey(c => c.ClassId);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Code)
            .HasMaxLength(50);

        builder.HasIndex(c => c.Name).IsUnique();

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");
    }
}
```

#### 2.4 Create ClassMembershipsConfiguration

**File**: `Accessors/DatabaseAccessor/DB/Configurations/ClassMembershipsConfiguration.cs`

```csharp
using DatabaseAccessor.Models.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseAccessor.DB.Configurations;

public class ClassMembershipsConfiguration : IEntityTypeConfiguration<ClassMembership>
{
    public void Configure(EntityTypeBuilder<ClassMembership> builder)
    {
        builder.ToTable("ClassMemberships");

        builder.HasKey(cm => new { cm.ClassId, cm.UserId, cm.Role });

        builder.Property(cm => cm.Role).IsRequired();
        builder.Property(cm => cm.AddedBy).IsRequired();
        builder.Property(cm => cm.AddedAt).IsRequired().HasDefaultValueSql("NOW()");

        builder.HasIndex(cm => new { cm.UserId, cm.Role, cm.ClassId });
        builder.HasIndex(cm => new { cm.ClassId, cm.Role, cm.UserId });

        builder.HasOne(cm => cm.Class)
            .WithMany(c => c.Memberships)
            .HasForeignKey(cm => cm.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.AddedByUser)
            .WithMany()
            .HasForeignKey(cm => cm.AddedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

#### 2.5 Create LessonsConfiguration

**File**: `Accessors/DatabaseAccessor/DB/Configurations/LessonsConfiguration.cs`

```csharp
using DatabaseAccessor.Models.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseAccessor.DB.Configurations;

public class LessonsConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");

        builder.HasKey(l => l.LessonId);

        builder.Property(l => l.ClassId).IsRequired();
        builder.Property(l => l.Title).IsRequired().HasMaxLength(150);
        builder.Property(l => l.ScheduledAt).IsRequired();
        builder.Property(l => l.Status).IsRequired().HasDefaultValue(0);
        builder.Property(l => l.CreatedBy).IsRequired();
        builder.Property(l => l.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");

        builder.HasIndex(l => l.ClassId);
        builder.HasIndex(l => new { l.ClassId, l.ScheduledAt });
        builder.HasIndex(l => l.CreatedBy);

        builder.HasOne(l => l.Class)
            .WithMany(c => c.Lessons)
            .HasForeignKey(l => l.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.CreatedByUser)
            .WithMany()
            .HasForeignKey(l => l.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

#### 2.6 Create LessonTasksConfiguration

**File**: `Accessors/DatabaseAccessor/DB/Configurations/LessonTasksConfiguration.cs`

```csharp
using DatabaseAccessor.Models.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class LessonTasksConfiguration : IEntityTypeConfiguration<LessonTask>
{
    public void Configure(EntityTypeBuilder<LessonTask> builder)
    {
        builder.ToTable("LessonTasks");

        builder.HasKey(lt => lt.LessonTaskId);

        builder.Property(lt => lt.LessonId).IsRequired();
        builder.Property(lt => lt.OrderIndex).IsRequired();
        builder.Property(lt => lt.Title).IsRequired().HasMaxLength(150);
        builder.Property(lt => lt.IsRequired).IsRequired().HasDefaultValue(true);

        builder.HasIndex(lt => lt.LessonId);
        builder.HasIndex(lt => new { lt.LessonId, lt.OrderIndex });

        builder.HasOne(lt => lt.Lesson)
            .WithMany(l => l.Tasks)
            .HasForeignKey(lt => lt.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### 2.7 Create LessonAttendancesConfiguration

**File**: `Accessors/DatabaseAccessor/DB/Configurations/LessonAttendancesConfiguration.cs`

```csharp
using DatabaseAccessor.Models.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class LessonAttendancesConfiguration : IEntityTypeConfiguration<LessonAttendance>
{
    public void Configure(EntityTypeBuilder<LessonAttendance> builder)
    {
        builder.ToTable("LessonAttendances");

        builder.HasKey(la => new { la.LessonId, la.UserId });

        builder.Property(la => la.Status).IsRequired().HasDefaultValue(0);

        builder.HasIndex(la => new { la.UserId, la.LessonId });
        builder.HasIndex(la => new { la.LessonId, la.Status });

        builder.HasOne(la => la.Lesson)
            .WithMany(l => l.Attendances)
            .HasForeignKey(la => la.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(la => la.User)
            .WithMany()
            .HasForeignKey(la => la.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### 2.8 Create LessonTaskProgressesConfiguration

**File**: `Accessors/DatabaseAccessor/DB/Configurations/LessonTaskProgressesConfiguration.cs`

```csharp
using DatabaseAccessor.Models.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class LessonTaskProgressesConfiguration : IEntityTypeConfiguration<LessonTaskProgress>
{
    public void Configure(EntityTypeBuilder<LessonTaskProgress> builder)
    {
        builder.ToTable("LessonTaskProgresses");

        builder.HasKey(lp => new { lp.LessonTaskId, lp.UserId });

        builder.Property(lp => lp.Status).IsRequired().HasDefaultValue(0);
        builder.Property(lp => lp.ProgressPercent).IsRequired().HasDefaultValue(0);
        builder.Property(lp => lp.LastUpdatedAt).IsRequired().HasDefaultValueSql("NOW()");

        builder.HasIndex(lp => new { lp.UserId, lp.LessonTaskId });
        builder.HasIndex(lp => new { lp.UserId, lp.Status });

        builder.HasOne(lp => lp.LessonTask)
            .WithMany()
            .HasForeignKey(lp => lp.LessonTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lp => lp.User)
            .WithMany()
            .HasForeignKey(lp => lp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### Phase 3: Create DbContext from Scratch

**File**: `Accessors/DatabaseAccessor/DB/DatabaseAccessorDbContext.cs`

```csharp
using DatabaseAccessor.DB.Configurations;
using DatabaseAccessor.Models.Users;
using DatabaseAccessor.Models.Classes;
using DatabaseAccessor.Models.Lessons;
using Microsoft.EntityFrameworkCore;

namespace DatabaseAccessor.DB;

public class DatabaseAccessorDbContext : DbContext
{
    public DatabaseAccessorDbContext(DbContextOptions<DatabaseAccessorDbContext> options)
        : base(options) { }

    // DbSets
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<UserDetails> UserDetails { get; set; } = default!;
    public DbSet<Class> Classes { get; set; } = default!;
    public DbSet<ClassMembership> ClassMemberships { get; set; } = default!;
    public DbSet<Lesson> Lessons { get; set; } = default!;
    public DbSet<LessonTask> LessonTasks { get; set; } = default!;
    public DbSet<LessonAttendance> LessonAttendances { get; set; } = default!;
    public DbSet<LessonTaskProgress> LessonTaskProgresses { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsersConfiguration());
        modelBuilder.ApplyConfiguration(new UserDetailsConfiguration());
        modelBuilder.ApplyConfiguration(new ClassesConfiguration());
        modelBuilder.ApplyConfiguration(new ClassMembershipsConfiguration());
        modelBuilder.ApplyConfiguration(new LessonsConfiguration());
        modelBuilder.ApplyConfiguration(new LessonTasksConfiguration());
        modelBuilder.ApplyConfiguration(new LessonAttendancesConfiguration());
        modelBuilder.ApplyConfiguration(new LessonTaskProgressesConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
```

### Phase 4: Create Initial Migration

#### 4.1 Configure DbContext in Program.cs

**File**: `Accessors/DatabaseAccessor/Program.cs`

Add EF Core configuration:

```csharp
using DatabaseAccessor.DB;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<DatabaseAccessorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ... rest of configuration
```

#### 4.2 Generate Initial Migration

```bash
cd backend/ContainerApp/Accessors/DatabaseAccessor
dotnet ef migrations add InitialCreate --context DatabaseAccessorDbContext
```

#### 4.3 Review Migration File

**File**: `Accessors/DatabaseAccessor/DB/Migrations/YYYYMMDDHHMMSS_InitialCreate.cs`

The migration will create all tables from scratch:
1. Users table (core auth only)
2. UserDetails table
3. Classes table
4. ClassMemberships table (with integer role)
5. Lessons table (with ClassId, Status, ScheduledAt, etc.)
6. LessonTasks table
7. LessonAttendances table
8. LessonTaskProgresses table

**Note**: This is a clean initial migration - no data migration needed.

#### 4.4 Apply Migration

```bash
dotnet ef database update --context DatabaseAccessorDbContext
```

**⚠️ Note**: This creates a fresh database structure. No existing data to migrate.

### Phase 5: Create Services from Scratch

#### 5.1 Create UserService

**File**: `Accessors/DatabaseAccessor/Services/Interfaces/IUserService.cs`

```csharp
namespace DatabaseAccessor.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct);
    Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken ct);
    Task<UserDetails?> GetUserDetailsAsync(Guid userId, CancellationToken ct);
    Task<UserDetails> CreateUserDetailsAsync(Guid userId, CreateUserDetailsRequest request, CancellationToken ct);
    Task<UserDetails> UpdateUserDetailsAsync(Guid userId, UpdateUserDetailsRequest request, CancellationToken ct);
}
```

**File**: `Accessors/DatabaseAccessor/Services/Implementations/UserService.cs`

Create implementation from scratch following patterns from original Accessor (as reference only).

#### 5.2 Create ClassService

**File**: `Accessors/DatabaseAccessor/Services/Interfaces/IClassService.cs`

```csharp
namespace DatabaseAccessor.Services.Interfaces;

public interface IClassService
{
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken ct);
    Task<Class> CreateClassAsync(CreateClassRequest request, CancellationToken ct);
    Task AddMembersToClassAsync(Guid classId, AddMembersRequest request, CancellationToken ct);
    // ... other methods
}
```

**File**: `Accessors/DatabaseAccessor/Services/Implementations/ClassService.cs`

Create implementation from scratch.

#### 5.3 Create LessonService

**File**: `Accessors/DatabaseAccessor/Services/Interfaces/ILessonService.cs`

```csharp
namespace DatabaseAccessor.Services.Interfaces;

public interface ILessonService
{
    Task<Lesson?> GetLessonByIdAsync(Guid lessonId, CancellationToken ct);
    Task<Lesson> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct);
    Task<LessonTask> AddTaskToLessonAsync(Guid lessonId, CreateLessonTaskRequest request, CancellationToken ct);
    Task<LessonAttendance> RecordAttendanceAsync(Guid lessonId, CreateAttendanceRequest request, CancellationToken ct);
    Task<LessonTaskProgress> RecordTaskProgressAsync(Guid taskId, CreateTaskProgressRequest request, CancellationToken ct);
    // ... other methods
}
```

**File**: `Accessors/DatabaseAccessor/Services/Implementations/LessonService.cs`

Create implementation from scratch.

### Phase 6: Create Mappers from Scratch

#### 6.1 Create UsersMapper

**File**: `Accessors/DatabaseAccessor/Mapping/UsersMapper.cs`

Create mapper methods for:
- User → UserResponse
- UserDetails → UserDetailsResponse
- CreateUserRequest → User + UserDetails
- UpdateUserDetailsRequest → UserDetails updates

#### 6.2 Create ClassesMapper

**File**: `Accessors/DatabaseAccessor/Mapping/ClassesMapper.cs`

Create mapper methods for:
- Class → ClassResponse
- ClassMembership → MemberResponse
- CreateClassRequest → Class

#### 6.3 Create LessonsMapper

**File**: `Accessors/DatabaseAccessor/Mapping/LessonsMapper.cs`

Create mapper methods for:
- Lesson → LessonResponse
- LessonTask → LessonTaskResponse
- LessonAttendance → LessonAttendanceResponse
- LessonTaskProgress → LessonTaskProgressResponse
- CreateLessonRequest → Lesson + LessonTasks

### Phase 7: Create Endpoints from Scratch

#### 7.1 Create UsersEndpoints

**File**: `Accessors/DatabaseAccessor/Endpoints/UsersEndpoints.cs`

Create endpoints:
- GET /users/{userId} - Get user with details
- POST /users - Create user with details
- PUT /users/{userId}/details - Update user details
- GET /users/{userId}/details - Get user details

#### 7.2 Create ClassesEndpoints

**File**: `Accessors/DatabaseAccessor/Endpoints/ClassesEndpoints.cs`

Create endpoints:
- GET /classes/{classId} - Get class with members
- POST /classes - Create class
- POST /classes/{classId}/members - Add members
- DELETE /classes/{classId}/members - Remove members

#### 7.3 Create LessonsEndpoints

**File**: `Accessors/DatabaseAccessor/Endpoints/LessonsEndpoints.cs`

Create endpoints:
- GET /lessons/{lessonId} - Get lesson with tasks
- POST /lessons - Create lesson
- POST /lessons/{lessonId}/tasks - Add task
- GET /lessons/{lessonId}/attendance - Get attendance
- POST /lessons/{lessonId}/attendance - Record attendance
- POST /lessons/tasks/{taskId}/progress - Record progress

### Phase 8: Create DTOs from Scratch

Create Request/Response DTOs as defined in contracts:
- `Accessors/DatabaseAccessor/Models/Users/Requests/CreateUserRequest.cs`
- `Accessors/DatabaseAccessor/Models/Users/Requests/CreateUserDetailsRequest.cs`
- `Accessors/DatabaseAccessor/Models/Users/Requests/UpdateUserDetailsRequest.cs`
- `Accessors/DatabaseAccessor/Models/Users/Responses/UserResponse.cs`
- `Accessors/DatabaseAccessor/Models/Users/Responses/UserDetailsResponse.cs`
- `Accessors/DatabaseAccessor/Models/Classes/Requests/CreateClassRequest.cs`
- `Accessors/DatabaseAccessor/Models/Classes/Requests/AddMembersRequest.cs`
- `Accessors/DatabaseAccessor/Models/Classes/Responses/ClassResponse.cs`
- `Accessors/DatabaseAccessor/Models/Lessons/Requests/CreateLessonRequest.cs`
- `Accessors/DatabaseAccessor/Models/Lessons/Requests/CreateLessonTaskRequest.cs`
- `Accessors/DatabaseAccessor/Models/Lessons/Requests/CreateAttendanceRequest.cs`
- `Accessors/DatabaseAccessor/Models/Lessons/Requests/CreateTaskProgressRequest.cs`
- `Accessors/DatabaseAccessor/Models/Lessons/Responses/LessonResponse.cs`
- `Accessors/DatabaseAccessor/Models/Lessons/Responses/LessonTaskResponse.cs`
- `Accessors/DatabaseAccessor/Models/Lessons/Responses/LessonAttendanceResponse.cs`
- `Accessors/DatabaseAccessor/Models/Lessons/Responses/LessonTaskProgressResponse.cs`
- etc.

### Phase 9: Testing

#### 9.1 Unit Tests

**File**: `UnitTests/DatabaseAccessorUnitTests/`

Create tests for:
- User and UserDetails operations
- Class and ClassMembership operations
- Lesson and LessonTask operations
- Attendance operations
- Progress operations

#### 9.2 Integration Tests

Test:
- Database context and migrations
- Endpoint functionality
- Service layer operations
- Repository layer operations

### Phase 10: Register Services

Update `Program.cs` to register all services:

```csharp
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ILessonService, LessonService>();

// Register repositories if using repository pattern
// Register mappers if using AutoMapper or similar
```

## Rollback Plan

If migration fails:

1. Revert migration: `dotnet ef database update <PreviousMigration> --context DatabaseAccessorDbContext`
2. Remove migration: `dotnet ef migrations remove --context DatabaseAccessorDbContext`
3. Fix issues and regenerate migration

## Verification Checklist

- [ ] All models created from scratch
- [ ] All configurations created from scratch
- [ ] DbContext created from scratch
- [ ] Initial migration generated and reviewed
- [ ] Migration applied successfully
- [ ] All services created from scratch
- [ ] All mappers created from scratch
- [ ] All endpoints created from scratch
- [ ] All DTOs created from scratch
- [ ] Services registered in Program.cs
- [ ] Unit tests created and pass
- [ ] Integration tests created and pass
- [ ] Documentation updated

## Important Notes

- **This is Version 2** - a completely new implementation
- **No code reuse** from original Accessor - everything created from scratch
- **Original Accessor** is used only as a reference for patterns and conventions
- **No data migration** - this is a fresh start
- **Independent service** - DatabaseAccessor operates independently

## Next Steps

After implementation:
1. Test all endpoints
2. Integrate with other services if needed
3. Update frontend to use new API structure
4. Monitor for any issues
5. Document API usage

