using CatalogService.DTOs;
using CatalogService.Entities;
namespace CatalogService.Services
{
    public interface IProductService
    {
        Task<List<ProductResponseDto>> GetAll();
        Task<ProductResponseDto> GetById(Guid id);
        Task<string> Create(CreateProductDto dto);
        Task<string> Update(Guid id, UpdateProductDto dto);

        Task<List<ProductResponseDto>> GetPaged(int page, int size);



        Task<List<AuditLog>> GetAuditPaged(int page, int size);

        Task<List<ProductResponseDto>> GetPLP(string? search, string? status, int page, int size, string? sort);

    }
}
