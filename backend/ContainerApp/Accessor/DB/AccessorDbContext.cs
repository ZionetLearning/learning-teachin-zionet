using Microsoft.EntityFrameworkCore;
using Accessor.Models;

namespace Accessor.DB;

public class AccessorDbContext : DbContext
{
    public AccessorDbContext(DbContextOptions<AccessorDbContext> options)
        : base(options) { }

    public DbSet<TaskModel> Tasks { get; set; } = default!;
    public DbSet<IdempotencyRecord> Idempotency { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TaskModel – ensure Id is unique/PK
        modelBuilder.Entity<TaskModel>(e =>
        {
            e.HasKey(t => t.Id);
            // configure other properties if needed
        });

        // Idempotency table
        modelBuilder.Entity<IdempotencyRecord>(e =>
        {
            e.ToTable("Idempotency");
            e.HasKey(i => i.IdempotencyKey);
            e.Property(i => i.IdempotencyKey).HasMaxLength(200).IsRequired();
            e.Property(i => i.Status).IsRequired();
            e.Property(i => i.CreatedAtUtc).IsRequired();
            // ExpiresAtUtc is optional
        });
    }
}
