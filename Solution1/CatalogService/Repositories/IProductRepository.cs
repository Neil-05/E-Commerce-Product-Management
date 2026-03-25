using CatalogService.Entities;

namespace CatalogService.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySKUAsync(string sku);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);

    Task<List<Product>> GetPagedAsync(int page, int size);

    Task<List<AuditLog>> GetAuditPagedAsync(int page, int size);

    Task<List<Product>> GetPLPAsync(string? search, string? status, int page, int size, string? sort);
}