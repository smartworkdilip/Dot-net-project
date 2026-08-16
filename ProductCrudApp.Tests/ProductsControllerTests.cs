using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductCrudApp.Api.Controllers;
using ProductCrudApp.Api.Dtos;
using ProductCrudApp.Api.Models;
using ProductCrudApp.Api.Services;

namespace ProductCrudApp.Tests;

// Unit tests for ProductsController's HTTP/status-code behavior, with
// IProductService mocked out so no business logic or DB is actually exercised.
public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithProducts()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Product>
        {
            new() { Id = 1, Name = "A", Price = 1m, StockQuantity = 1 },
            new() { Id = 2, Name = "B", Price = 2m, StockQuantity = 2 }
        });

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Equal(2, products.Count());
    }

    [Fact]
    public async Task GetLowStock_ReturnsOkWithProducts_UsingGivenThreshold()
    {
        _serviceMock.Setup(s => s.GetLowStockAsync(5)).ReturnsAsync(new List<Product>
        {
            new() { Id = 1, Name = "Almost Out", Price = 1m, StockQuantity = 1 }
        });

        var result = await _controller.GetLowStock(5);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Single(products);
        _serviceMock.Verify(s => s.GetLowStockAsync(5), Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenProductExists()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new Product { Id = 1, Name = "A", Price = 1m, StockQuantity = 1 });

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var product = Assert.IsType<ProductResponse>(okResult.Value);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProductMissing()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithNewProduct()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = 42; return p; });

        var request = new ProductCreateRequest { Name = "New", Price = 9.99m, StockQuantity = 3 };

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<ProductResponse>(createdResult.Value);
        Assert.Equal(42, product.Id);
        Assert.Equal("New", product.Name);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenProductExists()
    {
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<Product>())).ReturnsAsync(true);

        var result = await _controller.Update(1, new ProductUpdateRequest { Name = "Updated", Price = 5m, StockQuantity = 1 });

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenProductMissing()
    {
        _serviceMock.Setup(s => s.UpdateAsync(99, It.IsAny<Product>())).ReturnsAsync(false);

        var result = await _controller.Update(99, new ProductUpdateRequest { Name = "X", Price = 1m, StockQuantity = 1 });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenProductExists()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenProductMissing()
    {
        _serviceMock.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

        var result = await _controller.Delete(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
