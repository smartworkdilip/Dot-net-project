using ProductCrudApp.Api.Models;

namespace ProductCrudApp.Api.Services;

// Contract for the business/orchestration layer sitting between the controller
// and the repository. Lets ProductsController be unit-tested with a mocked service.
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);

    // Returns false (→ controller returns 404) if no product with this id exists.
    Task<bool> UpdateAsync(int id, Product product);

    // Returns false (→ controller returns 404) if no product with this id exists.
    Task<bool> DeleteAsync(int id);
}
