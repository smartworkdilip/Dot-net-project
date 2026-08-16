using Microsoft.EntityFrameworkCore;
using ProductCrudApp.Api.Models;

namespace ProductCrudApp.Api.Data;

// EF Core's session/unit-of-work: tracks entity changes and translates LINQ queries
// into SQL against the configured database (SQL Server, per Program.cs). Only the
// Repository layer talks to this directly.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Maps to the "Products" table.
    public DbSet<Product> Products => Set<Product>();

    // Fine-tunes the entity-to-table mapping beyond what data annotations express.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            // Ensures Price is stored as a fixed-precision SQL decimal(18,2)
            // rather than EF Core's default (which triggers a precision warning).
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
        });
    }
}
