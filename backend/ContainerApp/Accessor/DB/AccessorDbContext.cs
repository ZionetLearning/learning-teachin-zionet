using Accessor.DB.Configurations;
using Accessor.Models;
using Microsoft.EntityFrameworkCore;
using Accessor.Models.Users;
using Accessor.Models.Games;
using Accessor.Models.Prompts;
using Accessor.Models.Classes;
using Accessor.Models.WordCards;
using Accessor.Models.Meetings;
using Accessor.Models.GameConfiguration;
using Accessor.Models.Achievements;

namespace Accessor.DB;

public class AccessorDbContext : DbContext
{
    public AccessorDbContext(DbContextOptions<AccessorDbContext> options)
        : base(options) { }

    // DB tables
    public DbSet<TaskModel> Tasks { get; set; } = default!;
    public DbSet<ChatHistorySnapshot> ChatHistorySnapshots { get; set; } = default!;
    public DbSet<RefreshSessionsRecord> RefreshSessions { get; set; } = default!;
    public DbSet<UserModel> Users { get; set; } = default!;
    public DbSet<PromptModel> Prompts { get; set; } = default!;
    public DbSet<TeacherStudent> TeacherStudents { get; set; } = default!;
    public DbSet<GameAttempt> GameAttempts { get; set; } = default!;
    public DbSet<Class> Class { get; set; } = default!;
    public DbSet<ClassMembership> ClassMembership { get; set; } = default!;
    public DbSet<WordCardModel> WordCards { get; set; } = default!;
    public DbSet<MeetingModel> Meetings { get; set; } = default!;
    public DbSet<UserGameConfig> UserGameConfigs { get; set; } = default!;
    public DbSet<AchievementModel> Achievements { get; set; } = default!;
    public DbSet<UserAchievementModel> UserAchievements { get; set; } = default!;
    public DbSet<UserProgressModel> UserProgress { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ClassesConfiguration());
        modelBuilder.ApplyConfiguration(new ClassMembershipConfiguration());

        modelBuilder.ApplyConfiguration(new GameAttemptsConfiguration());

        // Users table
        modelBuilder.ApplyConfiguration(new UsersConfiguration());

        // Refresh Sessions table
        modelBuilder.ApplyConfiguration(new RefreshSessionConfiguration());

        // TaskModel – primary key + map Postgres system column `xmin` as a shadow concurrency token
        modelBuilder.Entity<TaskModel>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property<uint>("xmin")
             .HasColumnName("xmin")
             .IsConcurrencyToken()
             .ValueGeneratedOnAddOrUpdate();
        });

        // ChatHistorySnapshot table
        modelBuilder.Entity<ChatHistorySnapshot>(e =>
        {
            e.ToTable("ChatHistorySnapshots");
            e.HasKey(x => x.ThreadId);
            e.Property(x => x.UserId).IsRequired();
            e.Property(x => x.ChatType).HasDefaultValue("default");

            // jsonb в Postgres
            e.Property(x => x.History)
             .HasColumnType("jsonb")
             .IsRequired();

            e.Property(x => x.CreatedAt)
             .HasDefaultValueSql("NOW()");

            e.Property(x => x.UpdatedAt)
             .HasDefaultValueSql("NOW()");
        });
        modelBuilder.Entity<TeacherStudent>(e =>
        {
            e.ToTable("TeacherStudents");
            e.HasKey(x => new { x.TeacherId, x.StudentId });
            e.HasIndex(x => x.TeacherId);
            e.HasIndex(x => x.StudentId);
        });

        // Prompts table
        modelBuilder.Entity<PromptModel>(entity =>
        {
            entity.ToTable("Prompts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PromptKey)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(e => e.Version)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Content)
                .IsRequired();

            entity.HasIndex(e => new { e.PromptKey, e.Version });

            entity.HasIndex(e => e.PromptKey);
        });

        // Word cards table
        modelBuilder.ApplyConfiguration(new WordCardsConfiguration());

        // Meetings table
        modelBuilder.ApplyConfiguration(new MeetingsConfiguration());

        // User game configuration table
        modelBuilder.ApplyConfiguration(new UserGameConfigConfiguration());

        // Achievements tables
        modelBuilder.ApplyConfiguration(new AchievementsConfiguration());
        modelBuilder.ApplyConfiguration(new UserAchievementsConfiguration());
        modelBuilder.ApplyConfiguration(new UserProgressConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
