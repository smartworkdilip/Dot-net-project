using Microsoft.AspNetCore.Mvc;
using ProductCrudApp.Api.Dtos;
using ProductCrudApp.Api.Models;
using ProductCrudApp.Api.Services;

namespace ProductCrudApp.Api.Controllers;

// The "Controller" in MVC: the only layer that knows about HTTP. Handles request
// routing, binds/validates incoming JSON into DTOs, maps DTOs to/from the domain
// model, and translates service-layer results into status codes. All actual
// work is delegated to IProductService — no business or persistence logic here.
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // Maps the internal Product entity to the public-facing response DTO.
    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.CreatedAt);

    // GET /api/products — list every product.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products.Select(ToResponse));
    }

    // GET /api/products/{id} — fetch one product, or 404 if it doesn't exist.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(product));
    }

    // POST /api/products — create a new product; responds 201 with a Location
    // header pointing at GetById for the new resource.
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(ProductCreateRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        var created = await _productService.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToResponse(created));
    }

    // PUT /api/products/{id} — replace an existing product's fields; 204 on
    // success, 404 if the id doesn't exist.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductUpdateRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        var updated = await _productService.UpdateAsync(id, product);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE /api/products/{id} — remove a product; 204 on success, 404 if the
    // id doesn't exist.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
