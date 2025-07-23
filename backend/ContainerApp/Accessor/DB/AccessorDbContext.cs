using Microsoft.EntityFrameworkCore;
using Accessor.Models;

public class AccessorDbContext : DbContext
{
    public AccessorDbContext(DbContextOptions<AccessorDbContext> options)
        : base(options)
    {
    }


    // Define the DB  
    public DbSet<TaskModel> Tasks { get; set; }

}
// This class represents the database context for the Accessor service.