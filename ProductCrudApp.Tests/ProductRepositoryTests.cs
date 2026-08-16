using Microsoft.EntityFrameworkCore;
using ProductCrudApp.Api.Data;
using ProductCrudApp.Api.Models;
using ProductCrudApp.Api.Repositories;

namespace ProductCrudApp.Tests;

// Tests for ProductRepository's EF Core data access, run against an in-memory
// database provider (no real SQL Server needed for these to pass).
public class ProductRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ThenSaveChanges_PersistsProduct()
    {
        await using var context = CreateContext();
        var repository = new ProductRepository(context);

        var product = new Product { Name = "Keyboard", Price = 49.99m, StockQuantity = 10 };
        await repository.AddAsync(product);
        var saved = await repository.SaveChangesAsync();

        Assert.True(saved);
        Assert.NotEqual(0, product.Id);
        Assert.Equal(1, await context.Products.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersistedProducts()
    {
        await using var context = CreateContext();
        context.Products.AddRange(
            new Product { Name = "Mouse", Price = 19.99m, StockQuantity = 5 },
            new Product { Name = "Monitor", Price = 199.99m, StockQuantity = 3 });
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var result = await repository.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        await using var context = CreateContext();
        var product = new Product { Name = "Webcam", Price = 59.99m, StockQuantity = 8 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var result = await repository.GetByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal("Webcam", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var context = CreateContext();
        var repository = new ProductRepository(context);

        var result = await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLowStockAsync_ReturnsOnlyProductsAtOrBelowThreshold()
    {
        await using var context = CreateContext();
        context.Products.AddRange(
            new Product { Name = "Plenty", Price = 9.99m, StockQuantity = 50 },
            new Product { Name = "Exactly At Threshold", Price = 9.99m, StockQuantity = 5 },
            new Product { Name = "Almost Out", Price = 9.99m, StockQuantity = 1 },
            new Product { Name = "Out Of Stock", Price = 9.99m, StockQuantity = 0 });
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var result = (await repository.GetLowStockAsync(5)).ToList();

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, p => p.Name == "Plenty");
    }

    [Fact]
    public async Task UpdateAsync_ThenSaveChanges_PersistsModifications()
    {
        await using var context = CreateContext();
        var product = new Product { Name = "Old", Price = 10m, StockQuantity = 1 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var tracked = await repository.GetByIdAsync(product.Id);
        tracked!.Name = "New";
        await repository.UpdateAsync(tracked);
        await repository.SaveChangesAsync();

        var reloaded = await context.Products.FindAsync(product.Id);
        Assert.Equal("New", reloaded!.Name);
    }

    [Fact]
    public async Task DeleteAsync_ThenSaveChanges_RemovesProduct()
    {
        await using var context = CreateContext();
        var product = new Product { Name = "ToDelete", Price = 5m, StockQuantity = 1 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        await repository.DeleteAsync(product);
        await repository.SaveChangesAsync();

        Assert.Equal(0, await context.Products.CountAsync());
    }
}
