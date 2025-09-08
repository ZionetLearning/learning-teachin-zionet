using Accessor.DB.Configurations;
using Accessor.Models;
using Microsoft.EntityFrameworkCore;
using Accessor.Models.Users;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Users table
        modelBuilder.ApplyConfiguration(new UsersConfiguration());

        // Refresh Sessions table
        modelBuilder.ApplyConfiguration(new RefreshSessionConfiguration());

        // TaskModel – ensure Id is unique/PK
        modelBuilder.Entity<TaskModel>(e =>
        {
            e.HasKey(t => t.Id);
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
