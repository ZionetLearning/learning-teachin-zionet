using Microsoft.EntityFrameworkCore;
using Accessor.Models;

public class AccessorDbContext : DbContext
{
    public AccessorDbContext(DbContextOptions<AccessorDbContext> options)
        : base(options)
    {
    }


    // Define the DB name 
    public DbSet<TaskModel> Tasks { get; set; }

}
