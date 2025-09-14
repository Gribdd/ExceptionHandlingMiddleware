using ExceptionHandling.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExceptionHandling.Database;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) 
    : DbContext(options)
{
    public DbSet<Author> Authors { get; set; } = default!;
    public DbSet<Book> Books { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<AuditTrail> AuditTrails { get; set; } = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.AddInterceptors(new AuditableInterceptor());
        optionsBuilder.UseInMemoryDatabase("InMemoryDb");
    }
}
