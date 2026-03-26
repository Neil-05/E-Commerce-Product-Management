using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.Repositories;
using CatalogService.Data;
using System.Text.Json;

namespace CatalogService.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly IHttpContextAccessor _httpContext;

        public ProductService(
            IProductRepository repo,
            AppDbContext context,
            ILogger<ProductService> logger,
            IHttpContextAccessor httpContext)
        {
            _repo = repo;
            _context = context;
            _logger = logger;
            _httpContext = httpContext;
        }

        // ✅ GET ALL
        public async Task<List<ProductResponseDto>> GetAll()
        {
            var products = await _repo.GetAllAsync();

            return products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Status = p.Status,
                CreatedBy = p.CreatedBy
            }).ToList();
        }

        // ✅ GET BY ID
        public async Task<ProductResponseDto> GetById(Guid id)
        {
            var p = await _repo.GetByIdAsync(id);

            if (p == null)
                throw new Exception("Product not found");

            return new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Status = p.Status,
                CreatedBy = p.CreatedBy
            };
        }

        // 🔥 CREATE (FIXED)
        public async Task<string> Create(CreateProductDto dto)
        {
            
            var existing = await _repo.GetBySKUAsync(dto.SKU);
            if (existing != null)
                throw new Exception("SKU already exists");

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
                throw new Exception("Invalid CategoryId");

            // 🔥 FIX: get email from JWT
            var user = _httpContext.HttpContext?.User?
    .FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
    ?? "Unknown";

            var product = new Product
            {
                Name = dto.Name,
                SKU = dto.SKU,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                CreatedBy = user
            };

            await _repo.AddAsync(product);

            // ✅ AUDIT LOG
            var formatted =
"Product:\n" +
$"Name: {product.Name}\n" +
$"SKU: {product.SKU}\n" +
$"CategoryId: {product.CategoryId}\n" +
$"Description: {product.Description}\n" +
$"Status: {product.Status}\n\n" +
$"Created By: {user}\n" +
$"Date: {DateTime.UtcNow:yyyy-MM-dd}\n" +
$"Time: {DateTime.UtcNow:HH:mm:ss}";

            var audit = new AuditLog
            {
                Action = "CREATE",
                EntityName = "Product",
                OldValue = "",
                NewValue = formatted,
                CreatedBy = user
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created: {sku}", dto.SKU);

            return "Product Created";
        }

        // 🔥 UPDATE
        public async Task<string> Update(Guid id, UpdateProductDto dto)
        {
            var product = await _repo.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            var oldData = JsonSerializer.Serialize(product);

            product.Name = dto.Name;
            product.Description = dto.Description;

            await _repo.UpdateAsync(product);

            var audit = new AuditLog
            {
                Action = "UPDATE",
                EntityName = "Product",
                OldValue = oldData,
                NewValue = JsonSerializer.Serialize(product)
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();

            return "Product Updated";
        }

        // 🔥 PAGINATION
        public async Task<List<ProductResponseDto>> GetPaged(int page, int size)
        {
            var products = await _repo.GetPagedAsync(page, size);

            return products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Status = p.Status,
                CreatedBy = p.CreatedBy
            }).ToList();
        }

        public async Task<List<AuditLog>> GetAuditPaged(int page, int size)
        {
            return await _repo.GetAuditPagedAsync(page, size);
        }

        public async Task<List<ProductResponseDto>> GetPLP(string? search, string? status, int page, int size, string? sort)
        {
            var role = _httpContext.HttpContext?.User?
                .FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (role == "User" || string.IsNullOrEmpty(role))
            {
                status = "Approved";
            }

            var products = await _repo.GetPLPAsync(search, status, page, size, sort);

            return products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Status = p.Status,
                CreatedBy = p.CreatedBy
            }).ToList();
        }

        public async Task<string> UpdateStatus(Guid id, string status)
        {
            var product = await _repo.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            product.Status = status;

            await _repo.UpdateAsync(product);

            return "Status Updated";
        }
    }
}