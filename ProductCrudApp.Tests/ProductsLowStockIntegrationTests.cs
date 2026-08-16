using System.Net.Http.Json;
using ProductCrudApp.Api.Dtos;

namespace ProductCrudApp.Tests;

// Integration test: boots the real app end-to-end (real routing, model
// binding, DI, JSON serialization) against an in-memory database, instead of
// calling the controller action directly like ProductsControllerTests does.
public class ProductsLowStockIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsLowStockIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLowStock_ReturnsOnlyProductsAtOrBelowThreshold_OverRealHttpPipeline()
    {
        await _client.PostAsJsonAsync("/api/products",
            new ProductCreateRequest { Name = "Plenty", Price = 9.99m, StockQuantity = 50 });
        await _client.PostAsJsonAsync("/api/products",
            new ProductCreateRequest { Name = "Almost Out", Price = 9.99m, StockQuantity = 1 });

        var response = await _client.GetAsync("/api/products/low-stock?threshold=5");

        response.EnsureSuccessStatusCode();
        var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();

        Assert.NotNull(products);
        Assert.Contains(products!, p => p.Name == "Almost Out");
        Assert.DoesNotContain(products!, p => p.Name == "Plenty");
    }
}
