using Microsoft.EntityFrameworkCore;
using Accessor.Models;

namespace Accessor.DB;

public class AccessorDbContext : DbContext
{
    public AccessorDbContext(DbContextOptions<AccessorDbContext> options)
        : base(options) { }

    // DbSets
    public DbSet<TaskModel> Tasks { get; set; } = default!;
    public DbSet<ChatThread> ChatThreads { get; set; } = default!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = default!;
    public DbSet<IdempotencyRecord> Idempotency { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }
}
