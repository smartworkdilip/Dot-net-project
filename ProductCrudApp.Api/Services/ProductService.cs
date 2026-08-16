using ProductCrudApp.Api.Models;
using ProductCrudApp.Api.Repositories;

namespace ProductCrudApp.Api.Services;

// Business/orchestration layer (the "Service" in Controller-Service-Repository).
// Contains the logic for what a CRUD operation *means* (e.g. "update fails if the
// product doesn't exist") while delegating all persistence to IProductRepository.
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    // Straight pass-through: no business rules apply to a full listing.
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    // Straight pass-through: caller (controller) decides how to handle a null result.
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    // Straight pass-through: threshold interpretation is a query concern, not a business rule.
    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold)
    {
        return await _repository.GetLowStockAsync(threshold);
    }

    // Adds the new product and commits it in one step.
    public async Task<Product> CreateAsync(Product product)
    {
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
        return product;
    }

    // Loads the existing row, copies the new field values onto it, and saves.
    // Returns false if the id doesn't exist so the controller can return 404.
    public async Task<bool> UpdateAsync(int id, Product product)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;

        await _repository.UpdateAsync(existing);
        await _repository.SaveChangesAsync();
        return true;
    }

    // Loads the existing row and removes it. Returns false if the id doesn't
    // exist so the controller can return 404.
    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        await _repository.DeleteAsync(existing);
        await _repository.SaveChangesAsync();
        return true;
    }
}
