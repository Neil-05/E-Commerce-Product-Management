using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.Repositories;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;


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
            _logger.LogInformation("[PRODUCT][GET][START]");
            try
            {
                var products = await _repo.GetAllAsync();

                var result = products.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Status = p.Status,
                    CreatedBy = p.CreatedBy
                }).ToList();

                _logger.LogInformation("[PRODUCT][GET][SUCCESS] Count: {count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][GET][ERROR]");
                throw;
            }
        }

        // ✅ GET BY ID
        public async Task<ProductResponseDto> GetById(Guid id)
        {
            _logger.LogInformation("[PRODUCT][GET][START] Id: {id}", id);
            try
            {
                var p = await _repo.GetByIdAsync(id);

                if (p == null)
                {
                    _logger.LogWarning("[PRODUCT][GET][NOT_FOUND] Id: {id}", id);
                    throw new Exception("Product not found");
                }

                // 🔥 STEP 6 PART — FETCH IMAGES
                var media = await _context.MediaAssets
                    .Where(m => m.ProductId == id)
                    .Select(m => m.Url)
                    .ToListAsync();

                var dto = new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Status = p.Status,
                    CreatedBy = p.CreatedBy,
                    Images = media
                };

                _logger.LogInformation("[PRODUCT][GET][SUCCESS] Id: {id} SKU: {sku}", id, p.SKU);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][GET][ERROR] Id: {id}", id);
                throw;
            }
        }

        // 🔥 CREATE (FIXED)
        public async Task<string> Create(CreateProductDto dto)
        {
            _logger.LogInformation("[PRODUCT][CREATE][START] SKU: {sku}", dto.SKU);
            try
            {
                var existing = await _repo.GetBySKUAsync(dto.SKU);
                if (existing != null)
                {
                    _logger.LogWarning("[PRODUCT][CREATE][DUPLICATE_SKU] SKU: {sku}", dto.SKU);
                    throw new Exception("SKU already exists");
                }

                var category = await _context.Categories.FindAsync(dto.CategoryId);
                if (category == null)
                {
                    _logger.LogWarning("[PRODUCT][CREATE][INVALID_CATEGORY] CategoryId: {categoryId}", dto.CategoryId);
                    throw new Exception("Invalid CategoryId");
                }

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

                _logger.LogInformation("[PRODUCT][CREATE][SUCCESS] SKU: {sku} Id: {id}", dto.SKU, product.Id);

                return "Product Created";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][CREATE][ERROR] SKU: {sku}", dto.SKU);
                throw;
            }
        }

        // 🔥 UPDATE
        public async Task<string> Update(Guid id, UpdateProductDto dto)
        {
            _logger.LogInformation("[PRODUCT][UPDATE][START] Id: {id}", id);
            try
            {
                var product = await _repo.GetByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("[PRODUCT][UPDATE][NOT_FOUND] Id: {id}", id);
                    throw new Exception("Product not found");
                }

                if (product.Status == "Approved" || product.Status == "Published")
                {
                    _logger.LogWarning("[PRODUCT][UPDATE][INVALID_STATUS] Id: {id} Status: {status}", id, product.Status);
                    throw new Exception("Cannot update approved/published product");
                }

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

                _logger.LogInformation("[PRODUCT][UPDATE][SUCCESS] Id: {id} SKU: {sku}", id, product.SKU);

                return "Product Updated";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][UPDATE][ERROR] Id: {id}", id);
                throw;
            }
        }

        // 🔥 PAGINATION
        public async Task<List<ProductResponseDto>> GetPaged(int page, int size)
        {
            _logger.LogInformation("[PRODUCT][PAGED][START] Page: {page} Size: {size}", page, size);
            try
            {
                var products = await _repo.GetPagedAsync(page, size);

                var result = products.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Status = p.Status,
                    CreatedBy = p.CreatedBy
                }).ToList();

                _logger.LogInformation("[PRODUCT][PAGED][SUCCESS] Returned: {count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][PAGED][ERROR] Page: {page} Size: {size}", page, size);
                throw;
            }
        }

        public async Task<List<AuditLog>> GetAuditPaged(int page, int size)
        {
            _logger.LogInformation("[AUDIT][GET][START] Page: {page} Size: {size}", page, size);
            try
            {
                var result = await _repo.GetAuditPagedAsync(page, size);
                _logger.LogInformation("[AUDIT][GET][SUCCESS] Returned: {count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AUDIT][GET][ERROR] Page: {page} Size: {size}", page, size);
                throw;
            }
        }

        public async Task<List<ProductResponseDto>> GetPLP(string? search, string? status, int page, int size, string? sort)
        {
            _logger.LogInformation("[PRODUCT][PLP][START] Search: {search} Status: {status} Page: {page} Size: {size} Sort: {sort}", search, status, page, size, sort);
            try
            {
                var role = _httpContext.HttpContext?.User?
                    .FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (role == "User" || string.IsNullOrEmpty(role))
                {
                    status = "Approved";
                }

                var products = await _repo.GetPLPAsync(search, status, page, size, sort);

                var result = products.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Status = p.Status,
                    CreatedBy = p.CreatedBy
                }).ToList();

                _logger.LogInformation("[PRODUCT][PLP][SUCCESS] Returned: {count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][PLP][ERROR] Search: {search} Status: {status}", search, status);
                throw;
            }
        }

        public async Task<string> UpdateStatus(Guid id, string status)
        {
            _logger.LogInformation("[PRODUCT][UPDATE][STATUS][START] Id: {id} Status: {status}", id, status);
            try
            {
                var product = await _repo.GetByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("[PRODUCT][UPDATE][STATUS][NOT_FOUND] Id: {id}", id);
                    throw new Exception("Product not found");
                }

                product.Status = status;

                await _repo.UpdateAsync(product);

                _logger.LogInformation("[PRODUCT][UPDATE][STATUS][SUCCESS] Id: {id} Status: {status}", id, status);
                return "Status Updated";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][UPDATE][STATUS][ERROR] Id: {id} Status: {status}", id, status);
                throw;
            }
        }
        public async Task DeleteProduct(Guid id)
        {
            _logger.LogInformation("[PRODUCT][DELETE][START] Id: {id}", id);
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("[PRODUCT][DELETE][NOT_FOUND] Id: {id}", id);
                    throw new Exception("Product not found");
                }

                var userEmail = _httpContext.HttpContext.User
                    .FindFirst(ClaimTypes.Email)?.Value;

                var userRole = _httpContext.HttpContext.User
                    .FindFirst(ClaimTypes.Role)?.Value;

                // 🔥 MAIN LOGIC
                if (product.CreatedBy != userEmail && userRole != "Admin")
                {
                    _logger.LogWarning("[PRODUCT][DELETE][UNAUTHORIZED] Id: {id} User: {user}", id, userEmail);
                    throw new Exception("You are not allowed to delete this product");
                }

                // Optional rule
                if (product.Status == "Published")
                {
                    _logger.LogWarning("[PRODUCT][DELETE][INVALID_STATUS] Id: {id} Status: {status}", id, product.Status);
                    throw new Exception("Cannot delete published product");
                }

                product.IsDeleted = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation("[PRODUCT][DELETE][SUCCESS] Id: {id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][DELETE][ERROR] Id: {id}", id);
                throw;
            }
        }
        public async Task<List<ProductResponseDto>> GetDeletedProducts()
        {
            _logger.LogInformation("[PRODUCT][GET_DELETED][START]");
            try
            {
                var products = await _context.Products
                    .Where(p => p.IsDeleted)
                    .ToListAsync();

                var result = products.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Status = p.Status,
                    CreatedBy = p.CreatedBy
                }).ToList();

                _logger.LogInformation("[PRODUCT][GET_DELETED][SUCCESS] Count: {count}", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][GET_DELETED][ERROR]");
                throw;
            }
        }

        public async Task<string> RestoreProduct(Guid id)
        {
            _logger.LogInformation("[PRODUCT][RESTORE][START] Id: {id}", id);
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("[PRODUCT][RESTORE][NOT_FOUND] Id: {id}", id);
                    throw new Exception("Product not found");
                }

                if (!product.IsDeleted)
                {
                    _logger.LogWarning("[PRODUCT][RESTORE][NOT_DELETED] Id: {id}", id);
                    throw new Exception("Product is not deleted");
                }

                product.IsDeleted = false;

                await _context.SaveChangesAsync();

                _logger.LogInformation("[PRODUCT][RESTORE][SUCCESS] Id: {id}", id);
                return "Product restored";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PRODUCT][RESTORE][ERROR] Id: {id}", id);
                throw;
            }
        }
        public async Task<string> AddMedia(Guid productId, string url)
        {
            _logger.LogInformation("[IMAGE][ADD][START] ProductId: {productId} Url: {url}", productId, url);
            try
            {
                var product = await _repo.GetByIdAsync(productId);

                if (product == null)
                {
                    _logger.LogWarning("[IMAGE][ADD][NOT_FOUND] ProductId: {productId}", productId);
                    throw new Exception("Product not found");
                }

                var media = new MediaAsset
                {
                    ProductId = productId,
                    Url = url
                };

                _context.MediaAssets.Add(media);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[IMAGE][ADD][SUCCESS] ProductId: {productId} MediaId: {mediaId}", productId, media.Id);
                return "Media added";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IMAGE][ADD][ERROR] ProductId: {productId}", productId);
                throw;
            }
        }

        public async Task<string> UploadImage(Guid productId, IFormFile file, bool isPrimary)
        {
            _logger.LogInformation("[IMAGE][UPLOAD][START] ProductId: {productId} FileName: {fileName} IsPrimary: {isPrimary}", productId, file?.FileName, isPrimary);
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("[IMAGE][UPLOAD][INVALID_FILE] ProductId: {productId}", productId);
                    throw new Exception("Invalid file");
                }

                var product = await _repo.GetByIdAsync(productId);

                if (product == null)
                {
                    _logger.LogWarning("[IMAGE][UPLOAD][NOT_FOUND] ProductId: {productId}", productId);
                    throw new Exception("Product not found");
                }

                // 🔥 WORKFLOW RULE
                if (product.Status != "Draft" && product.Status != "Enrichment")
                {
                    _logger.LogWarning("[IMAGE][UPLOAD][INVALID_STAGE] ProductId: {productId} Status: {status}", productId, product.Status);
                    throw new Exception("Cannot upload images at this stage");
                }

                // 🔥 SAVE FILE
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"/images/{fileName}";

                var hasPrimary = await _context.MediaAssets
                    .AnyAsync(m => m.ProductId == productId && m.IsPrimary);

                if (!hasPrimary)
                {
                    isPrimary = true;
                }

                // 🔥 IF PRIMARY → REMOVE OLD PRIMARY
                if (isPrimary)
                {
                    var existingPrimary = await _context.MediaAssets
                        .Where(m => m.ProductId == productId && m.IsPrimary)
                        .ToListAsync();

                    foreach (var m in existingPrimary)
                    {
                        m.IsPrimary = false;
                    }

                    _logger.LogInformation("[IMAGE][UPLOAD][PRIMARY_REMOVED] ProductId: {productId} Removed: {count}", productId, existingPrimary.Count);
                }

                // 🔥 SAVE NEW IMAGE
                var media = new MediaAsset
                {
                    ProductId = productId,
                    Url = url,
                    IsPrimary = isPrimary
                };

                _context.MediaAssets.Add(media);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[IMAGE][UPLOAD][SUCCESS] ProductId: {productId} MediaId: {mediaId} Url: {url}", productId, media.Id, url);

                return "Image uploaded";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IMAGE][UPLOAD][ERROR] ProductId: {productId}", productId);
                throw;
            }
        }
        public async Task<string> DeleteImage(Guid mediaId)
        {
            _logger.LogInformation("[IMAGE][DELETE][START] MediaId: {mediaId}", mediaId);
            try
            {
                var media = await _context.MediaAssets.FindAsync(mediaId);

                if (media == null)
                {
                    _logger.LogWarning("[IMAGE][DELETE][NOT_FOUND] MediaId: {mediaId}", mediaId);
                    throw new Exception("Image not found");
                }

                // 🔥 delete file from disk
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    media.Url.TrimStart('/')
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("[IMAGE][DELETE][FILE_REMOVED] MediaId: {mediaId} Path: {path}", mediaId, filePath);
                }

                _context.MediaAssets.Remove(media);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[IMAGE][DELETE][SUCCESS] MediaId: {mediaId}", mediaId);
                return "Image deleted";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IMAGE][DELETE][ERROR] MediaId: {mediaId}", mediaId);
                throw;
            }
        }
    }

}