using CatalogService.DTOs;
namespace CatalogService.Services
{
    public interface IProductService
    {
        Task<List<ProductResponseDto>> GetAll();
        Task<ProductResponseDto> GetById(Guid id);
        Task<string> Create(CreateProductDto dto);
        Task<string> Update(Guid id, UpdateProductDto dto);

    }
}
