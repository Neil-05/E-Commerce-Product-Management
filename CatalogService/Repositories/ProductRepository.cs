using CatalogService.Data;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
        .Where(p => !p.IsDeleted)
        .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }
    
    public async Task<Product?> GetBySKUAsync(string sku)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Product>> GetPagedAsync(int page, int size)
    {
        return await _context.Products
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetAuditPagedAsync(int page, int size)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<List<Product>> GetPLPAsync(string? search, string? status, int page, int size, string? sort)
    {
        var query = _context.Products.AsQueryable();

        // 🔍 Search
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));
        }

        // 🎯 Filter
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status == status);
        }

        // 🔃 Sorting
        query = sort switch
        {
            "name" => query.OrderBy(p => p.Name),
            "sku" => query.OrderBy(p => p.SKU),
            _ => query.OrderByDescending(p => p.Id)
        };

        return await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }
}