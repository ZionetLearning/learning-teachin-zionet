using Microsoft.EntityFrameworkCore;
using Accessor.Models;

namespace Accessor.DB;
public class AccessorDbContext : DbContext
{
    public AccessorDbContext(DbContextOptions<AccessorDbContext> options)
        : base(options)
    {
    }

    // Define the DB  
    public DbSet<TaskModel> Tasks { get; set; } = null!;
    public DbSet<ChatThread> ChatThreads { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatThread>()
            .HasMany(t => t.Messages)
            .WithOne(m => m.Thread)
            .HasForeignKey(m => m.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasIndex(m => m.ThreadId);

        base.OnModelCreating(modelBuilder);
    }
}
// This class represents the database context for the Accessor service.