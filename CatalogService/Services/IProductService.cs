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
        Task<string> UpdateStatus(Guid id, string status);
        Task DeleteProduct(Guid id);
        Task<List<ProductResponseDto>> GetDeletedProducts();
        Task<string> RestoreProduct(Guid id);
        Task<string> AddMedia(Guid productId, string url);
        Task<string> UploadImage(Guid productId, IFormFile file, bool isPrimary);
        Task<string> DeleteImage(Guid mediaId);

    }
}
