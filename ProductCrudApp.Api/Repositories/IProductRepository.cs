using ProductCrudApp.Api.Models;

namespace ProductCrudApp.Api.Repositories;

// Contract for the data-access layer. Abstracts EF Core away from ProductService,
// so the service can be unit-tested with a mocked repository instead of a real DB.
public interface IProductRepository
{
    // Reads every row from the Products table.
    Task<IEnumerable<Product>> GetAllAsync();

    // Reads a single row by primary key, or null if it doesn't exist.
    Task<Product?> GetByIdAsync(int id);

    // Stages a new entity for insertion (not persisted until SaveChangesAsync).
    Task<Product> AddAsync(Product product);

    // Marks an existing (tracked) entity as modified.
    Task UpdateAsync(Product product);

    // Stages an entity for deletion.
    Task DeleteAsync(Product product);

    // Commits all staged Add/Update/Delete operations to the database in one transaction.
    Task<bool> SaveChangesAsync();
}
