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
    public DbSet<TaskModel> Tasks { get; set; }
    public DbSet<ChatThread> ChatThreads { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
}
// This class represents the database context for the Accessor service.