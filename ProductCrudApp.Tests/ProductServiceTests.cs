using Moq;
using ProductCrudApp.Api.Models;
using ProductCrudApp.Api.Repositories;
using ProductCrudApp.Api.Services;

namespace ProductCrudApp.Tests;

// Unit tests for ProductService's business logic, with IProductRepository mocked
// out so the database is never touched.
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock = new();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_AddsProduct_AndSavesChanges()
    {
        var product = new Product { Name = "Keyboard", Price = 49.99m, StockQuantity = 10 };
        _repositoryMock.Setup(r => r.AddAsync(product)).ReturnsAsync(product);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var created = await _service.CreateAsync(product);

        Assert.Equal("Keyboard", created.Name);
        _repositoryMock.Verify(r => r.AddAsync(product), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProductsFromRepository()
    {
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>
        {
            new() { Id = 1, Name = "Mouse", Price = 19.99m, StockQuantity = 5 },
            new() { Id = 2, Name = "Monitor", Price = 199.99m, StockQuantity = 3 }
        });

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product { Id = 1, Name = "Webcam", Price = 59.99m, StockQuantity = 8 });

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Webcam", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetLowStockAsync_ReturnsProductsFromRepository_ForGivenThreshold()
    {
        _repositoryMock.Setup(r => r.GetLowStockAsync(5)).ReturnsAsync(new List<Product>
        {
            new() { Id = 1, Name = "Almost Out", Price = 9.99m, StockQuantity = 1 }
        });

        var result = await _service.GetLowStockAsync(5);

        Assert.Single(result);
        _repositoryMock.Verify(r => r.GetLowStockAsync(5), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingProduct_AndReturnsTrue()
    {
        var existing = new Product { Id = 1, Name = "Old Name", Price = 10m, StockQuantity = 1 };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var updated = await _service.UpdateAsync(1, new Product
        {
            Name = "New Name",
            Description = "Updated",
            Price = 20m,
            StockQuantity = 2
        });

        Assert.True(updated);
        Assert.Equal("New Name", existing.Name);
        Assert.Equal(20m, existing.Price);
        _repositoryMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(123)).ReturnsAsync((Product?)null);

        var updated = await _service.UpdateAsync(123, new Product { Name = "X", Price = 1m, StockQuantity = 1 });

        Assert.False(updated);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct_AndReturnsTrue()
    {
        var existing = new Product { Id = 1, Name = "ToDelete", Price = 5m, StockQuantity = 1 };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var deleted = await _service.DeleteAsync(1);

        Assert.True(deleted);
        _repositoryMock.Verify(r => r.DeleteAsync(existing), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(456)).ReturnsAsync((Product?)null);

        var deleted = await _service.DeleteAsync(456);

        Assert.False(deleted);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
    }
}
