using Microsoft.EntityFrameworkCore;
using ProductCrudApp.Api.Data;
using ProductCrudApp.Api.Models;

namespace ProductCrudApp.Api.Repositories;

// EF Core implementation of IProductRepository. The only class in the app that
// issues queries/commands against AppDbContext — all SQL-generating code lives here.
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    // AsNoTracking: this is a read-only listing, so EF Core skips change-tracking
    // overhead for the returned entities.
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.AsNoTracking().ToListAsync();
    }

    // Tracked (not AsNoTracking) because the service layer may mutate and save
    // this same instance for update/delete flows.
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    // Read-only, so AsNoTracking like GetAllAsync.
    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.StockQuantity <= threshold)
            .ToListAsync();
    }

    // Queues an INSERT; the row isn't written until SaveChangesAsync runs.
    public async Task<Product> AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        return product;
    }

    // Marks all scalar properties as modified, queuing an UPDATE on save.
    public Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    // Queues a DELETE on save.
    public Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        return Task.CompletedTask;
    }

    // Flushes all queued Add/Update/Remove operations to SQL Server as one batch.
    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() >= 0;
    }
}
