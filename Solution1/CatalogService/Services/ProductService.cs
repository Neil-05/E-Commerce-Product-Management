using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.Repositories;

namespace CatalogService.Services
{
    public class ProductService: IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repo, ILogger<ProductService> logger)
        {
            _repo = repo;
            _logger = logger;

        }
        public async Task<List<ProductResponseDto>> GetAll()
        {
            var products = await _repo.GetAllAsync();

            return products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name= p.Name,
                SKU= p.SKU,
                Status = p.Status

            }).ToList();
        }
        public async Task<ProductResponseDto> GetById(Guid id)
        {
            var p = await _repo.GetByIdAsync(id);

            return new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Status = p.Status
            };
        }

        public async Task<string> Create(CreateProductDto dto)
        {
            var existing = await _repo.GetBySKUAsync(dto.SKU);
            if (existing != null)
                throw new Exception("SKU already exists");

            var product = new Product
            {
                Name = dto.Name,
                SKU = dto.SKU,
                CategoryId = dto.CategoryId,
                Description = dto.Description
            };

            await _repo.AddAsync(product);

            _logger.LogInformation("Product created: {sku}", dto.SKU);

            return "Product Created";
        }

        public async Task<string> Update(Guid id, UpdateProductDto dto)
        {
            var product = await _repo.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            product.Name = dto.Name;
            product.Description = dto.Description;

            await _repo.UpdateAsync(product);

            return "Product Updated";
        }
    }
}
