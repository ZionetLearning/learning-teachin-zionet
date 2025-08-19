using Accessor.DB.Configurations;
using Accessor.Models;
using Microsoft.EntityFrameworkCore;

namespace Accessor.DB;

public class AccessorDbContext : DbContext
{
    public AccessorDbContext(DbContextOptions<AccessorDbContext> options)
        : base(options) { }

    // DB tables
    public DbSet<TaskModel> Tasks { get; set; } = default!;
    public DbSet<ChatThread> ChatThreads { get; set; } = default!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = default!;
    public DbSet<ChatHistorySnapshot> ChatHistorySnapshots { get; set; } = default!;
    public DbSet<IdempotencyRecord> Idempotency { get; set; } = default!;
    public DbSet<RefreshSessionsRecord> RefreshSessions { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User table configuration
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Refresh Sessions table
        modelBuilder.ApplyConfiguration(new RefreshSessionConfiguration());

        // TaskModel – ensure Id is unique/PK
        modelBuilder.Entity<TaskModel>(e =>
        {
            e.HasKey(t => t.Id);
        });

        // ChatThread -> ChatMessage relationship with cascade delete
        modelBuilder.Entity<ChatThread>()
            .HasMany(t => t.Messages)
            .WithOne(m => m.Thread)
            .HasForeignKey(m => m.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on ChatMessage.ThreadId
        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => m.ThreadId);

        // Idempotency table
        modelBuilder.Entity<IdempotencyRecord>(e =>
        {
            e.ToTable("Idempotency");
            e.HasKey(i => i.IdempotencyKey);
            e.Property(i => i.IdempotencyKey).HasMaxLength(200).IsRequired();
            e.Property(i => i.Status).IsRequired();
            e.Property(i => i.CreatedAtUtc).IsRequired();
            // ExpiresAtUtc optional
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

        base.OnModelCreating(modelBuilder);
    }
}
