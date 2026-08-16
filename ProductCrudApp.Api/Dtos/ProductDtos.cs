using System.ComponentModel.DataAnnotations;

namespace ProductCrudApp.Api.Dtos;

// Data Transfer Objects: shape the JSON that crosses the wire, kept separate from
// the Product entity so the API contract can evolve independently of the DB schema.

// What gets serialized back to the client for every read/create/update response.
public record ProductResponse(int Id, string Name, string? Description, decimal Price, int StockQuantity, DateTime CreatedAt);

// Expected request body for POST /api/products (create). No Id/CreatedAt — those
// are server-assigned.
public class ProductCreateRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}

// Expected request body for PUT /api/products/{id} (update). Same shape as create
// since every field is replaceable; the target row is identified by the route id.
public class ProductUpdateRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}
