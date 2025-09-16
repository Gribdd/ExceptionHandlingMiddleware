using ExceptionHandling.Database.Entities;
using ExceptionHandling.Database.Mapping;
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

        modelBuilder.ApplyConfiguration(new BookConfiguration());
        modelBuilder.ApplyConfiguration(new AuthorConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new AuditTrailConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        optionsBuilder.UseSqlite($"Data Source={System.IO.Path.Join(path, "test.db")}");
        optionsBuilder.AddInterceptors(new AuditableInterceptor());
    }
}
