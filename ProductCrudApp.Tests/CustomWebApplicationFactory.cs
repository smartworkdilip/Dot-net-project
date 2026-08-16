using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProductCrudApp.Api.Data;

namespace ProductCrudApp.Tests;

// Boots the real ASP.NET Core app (via the partial Program class Program.cs
// exposes) for integration tests, swapping the SQL Server AppDbContext
// registration for EF Core's InMemory provider so tests need no real database.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // On current EF Core, AddDbContext's configuration is stored as
            // IDbContextOptionsConfiguration<T> entries that ACCUMULATE across
            // multiple AddDbContext calls rather than being replaced — removing
            // just DbContextOptions<AppDbContext>/AppDbContext isn't enough, the
            // original UseSqlServer configuration survives via that interface
            // and gets combined with UseInMemoryDatabase below. Removing it too
            // is what actually makes this a replacement instead of an addition.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
