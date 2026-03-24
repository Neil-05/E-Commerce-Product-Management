using CatalogService.Entities;

namespace CatalogService.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySKUAsync(string sku);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
}