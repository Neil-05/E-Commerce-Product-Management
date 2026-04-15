using CatalogService.DTOs;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _service.GetById(id));
        }

        [Authorize(Roles = "ProductManager,Admin,Content-Executive")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            return Ok(await _service.Create(dto));
        }

        [Authorize(Roles = "ProductManager,Admin,Content-Executive")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateProductDto dto)
        {
            return Ok(await _service.Update(id, dto));
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            return Ok(await _service.GetPaged(page, size));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("audit/paged")]
        public async Task<IActionResult> GetAuditPaged(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var data = await _service.GetAuditPaged(page, size);
            return Ok(data);
        }

        [HttpGet("plp")]
        public async Task<IActionResult> GetPLP(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? sort = null)
        {
            var data = await _service.GetPLP(search, status, page, size, sort);
            return Ok(data);
        }

        // 🔥 ONLY CHANGE IS HERE (VERY IMPORTANT)
        //[Authorize(Roles = "ProductManager,Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            var result = await _service.UpdateStatus(id, dto.Status);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteProduct(id);
            return Ok("Product deleted");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _service.GetDeletedProducts();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var result = await _service.RestoreProduct(id);
            return Ok(result);
        }

        [HttpPost("{id}/media")]
        public async Task<IActionResult> AddMedia(Guid id, AddMediaDto dto)
        {
            var result = await _service.AddMedia(id, dto.Url);
            return Ok(result);
        }


        [Authorize(Roles = "ContentExecutive,Admin")]
        [HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile file, bool isPrimary = false)
        {
            var result = await _service.UploadImage(id, file, isPrimary);
            return Ok(result);
        }


        [Authorize(Roles = "ContentExecutive,Admin")]
        [HttpDelete("media/{mediaId}")]
        public async Task<IActionResult> DeleteImage(Guid mediaId)
        {
            var result = await _service.DeleteImage(mediaId);
            return Ok(result);
        }
    }
}