using System.Net.Http.Json;
using WorkflowService.Data;
using WorkflowService.Dtos;
using WorkflowService.Entitites;
using WorkflowService.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WorkflowService.Services
{
    public class WorkflowManager : IWorkflowService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WorkflowManager> _logger;

        public WorkflowManager(
            AppDbContext context,
            IHttpContextAccessor httpContext,
            IHttpClientFactory factory,
            ILogger<WorkflowManager> logger)
        {
            _context = context;
            _httpContext = httpContext;
            _httpClient = factory.CreateClient("catalog");
            _logger = logger;
        }

        private string GetUser()
        {
            return _httpContext.HttpContext?.User?.Identity?.Name ?? "Unknown";
        }

        // 🔥 GET CURRENT STATUS FROM CATALOG
        private async Task<string> GetCurrentStatus(Guid productId)
        {
            _logger.LogInformation("[Workflow][WORKFLOW][STATUS][GET][START] ProductId: {productId} User: {user} ⇢", productId, GetUser());
            try
            {
                var product = await _httpClient.GetFromJsonAsync<ProductDto>(
                    $"/api/products/{productId}"
                );

                if (product == null)
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][STATUS][GET][NOT_FOUND] ProductId: {productId} ⇢", productId);
                    throw new Exception("Product not found");
                }

                _logger.LogInformation("[Workflow][WORKFLOW][STATUS][GET][SUCCESS] ProductId: {productId} Status: {status} ⇢", productId, product.Status);
                return product.Status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow][WORKFLOW][STATUS][GET][ERROR] ProductId: {productId} ✖ ⇢", productId);
                throw;
            }
        }

        // 🔥 SUBMIT (Draft → InReview)
        public async Task<string> Submit(Guid productId)
        {
            _logger.LogInformation("[Workflow][WORKFLOW][STATUS][START] Submit ProductId: {productId} User: {user} ⇢", productId, GetUser());
            try
            {
                var currentStatus = await GetCurrentStatus(productId);

                if (currentStatus != "Draft")
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][STATUS][INVALID] Submit ProductId: {productId} CurrentStatus: {status} ⇢", productId, currentStatus);
                    throw new Exception("Only Draft products can be submitted");
                }

                await UpdateStatus(productId, "InReview");

                _logger.LogInformation("[Workflow][WORKFLOW][STATUS][SUCCESS] Submit ProductId: {productId} → InReview ✓ ⇢", productId);
                return "Submitted for review";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow][WORKFLOW][STATUS][ERROR] Submit ProductId: {productId} ✖ ⇢", productId);
                throw;
            }
        }

        // 🔥 APPROVE (InReview → Approved)
        public async Task<string> Approve(Guid productId)
        {
            _logger.LogInformation("[Workflow][WORKFLOW][APPROVE][START] ProductId: {productId} User: {user} ⇢", productId, GetUser());
            try
            {
                var currentStatus = await GetCurrentStatus(productId);

                if (currentStatus != "InReview")
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][APPROVE][INVALID] ProductId: {productId} CurrentStatus: {status} ⇢", productId, currentStatus);
                    throw new Exception("Only InReview products can be approved");
                }

                await UpdateStatus(productId, "Approved");

                _logger.LogInformation("[Workflow][WORKFLOW][APPROVE][SUCCESS] ProductId: {productId} → Approved ✓ ⇢", productId);
                return "Product Approved";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow][WORKFLOW][APPROVE][ERROR] ProductId: {productId} ✖ ⇢", productId);
                throw;
            }
        }

        // 🔥 REJECT (InReview → Rejected)
        public async Task<string> Reject(Guid productId)
        {
            _logger.LogInformation("[Workflow][WORKFLOW][REJECT][START] ProductId: {productId} User: {user} ⇢", productId, GetUser());
            try
            {
                var currentStatus = await GetCurrentStatus(productId);

                if (currentStatus != "InReview")
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][REJECT][INVALID] ProductId: {productId} CurrentStatus: {status} ⇢", productId, currentStatus);
                    throw new Exception("Only InReview products can be rejected");
                }

                await UpdateStatus(productId, "Rejected");

                _logger.LogInformation("[Workflow][WORKFLOW][REJECT][SUCCESS] ProductId: {productId} → Rejected ✓ ⇢", productId);
                return "Product Rejected";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow][WORKFLOW][REJECT][ERROR] ProductId: {productId} ✖ ⇢", productId);
                throw;
            }
        }

        // 🔥 COMMON STATUS UPDATE METHOD
        private async Task UpdateStatus(Guid productId, string status)
        {
            _logger.LogInformation("[Workflow][WORKFLOW][STATUS][UPDATE][START] ProductId: {productId} → {status} User: {user} ⇢", productId, status, GetUser());
            try
            {
                var workflow = new Workflow
                {
                    ProductId = productId,
                    Status = status,
                    ActionBy = GetUser(),
                    Timestamp = DateTime.UtcNow
                };

                _context.Workflows.Add(workflow);
                await _context.SaveChangesAsync();

                var response = await _httpClient.PutAsJsonAsync(
                    $"/api/products/{productId}/status",
                    new { status }
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][STATUS][UPDATE][FAILED] ProductId: {productId} Status: {status} ⇢", productId, status);
                    throw new Exception("Catalog update failed");
                }

                _logger.LogInformation("[Workflow][WORKFLOW][STATUS][UPDATE][SUCCESS] ProductId: {productId} → {status} ✓ ⇢", productId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow][WORKFLOW][STATUS][UPDATE][ERROR] ProductId: {productId} → {status} ✖ ⇢", productId, status);
                throw;
            }
        }
        public async Task<string> Publish(Guid productId)
        {
            _logger.LogInformation("[Workflow][WORKFLOW][PUBLISH][START] ProductId: {productId} User: {user} ⇢", productId, GetUser());
            try
            {
                var currentStatus = await GetCurrentStatus(productId);

                if (currentStatus != "Approved")
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][PUBLISH][INVALID] ProductId: {productId} CurrentStatus: {status} ⇢", productId, currentStatus);
                    throw new Exception("Only Approved products can be published");
                }

                var role = _httpContext.HttpContext?.User?
                    .FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (role != "Admin")
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][PUBLISH][FORBIDDEN] ProductId: {productId} Role: {role} ⇢", productId, role);
                    throw new Exception("Only Admin can publish products");
                }

                // ✅ 1. Update Catalog + DB
                await UpdateStatus(productId, "Published");

                // ✅ 2. 🔥 ADD RABBITMQ HERE
                var publisher = new RabbitMQPublisher();
                var product = await _httpClient.GetFromJsonAsync<ProductDto>(
                    $"/api/products/{productId}"
                );

                publisher.Publish(new
                {
                    ProductId = productId,
                    Status = "Published",
                    Email = product.CreatedBy,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("[Workflow][WORKFLOW][PUBLISH][SUCCESS] ProductId: {productId} → Published ✓ ⇢", productId);
                return "Product Published";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow][WORKFLOW][PUBLISH][ERROR] ProductId: {productId} ✖ ⇢", productId);
                throw;
            }
        }

        public async Task<List<WorkflowHistoryDto>> GetHistory(Guid productId)
        {
            _logger.LogInformation("[Workflow][WORKFLOW][HISTORY][START] ProductId: {productId} ⇢", productId);
            try
            {
                var data = await _context.Workflows
                    .Where(x => x.ProductId == productId)
                    .OrderBy(x => x.Timestamp)
                    .ToListAsync();

                var result = data.Select(x => new WorkflowHistoryDto
                {
                    Status = x.Status,
                    ActionBy = x.ActionBy,
                    Timestamp = x.Timestamp
                }).ToList();

                _logger.LogInformation("[Workflow][WORKFLOW][HISTORY][SUCCESS] ProductId: {productId} Count: {count} ⇢", productId, result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow][WORKFLOW][HISTORY][ERROR] ProductId: {productId} ✖ ⇢", productId);
                throw;
            }
        }

        public async Task<string> UpdatePricing(Guid productId, PricingDto dto)
        {
            _logger.LogInformation("[Workflow][WORKFLOW][PRICING][START] ProductId: {productId} User: {user} ⇢", productId, _httpContext.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value);
            try
            {
                var product = await _httpClient.GetFromJsonAsync<ProductDto>(
                    $"/api/products/{productId}"
                );

                var user = _httpContext.HttpContext?.User?
                    .FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                // 🔥 OWNER CHECK
                if (product.CreatedBy != user)
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][PRICING][FORBIDDEN] ProductId: {productId} User: {user} ⇢", productId, user);
                    throw new Exception("Only creator can update pricing");
                }

                // OPTIONAL: status check
                if (product.Status != "Draft")
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][PRICING][INVALID_STAGE] ProductId: {productId} Status: {status} ⇢", productId, product.Status);
                    throw new Exception("Can only update pricing in Draft state");
                }

                _logger.LogInformation("[Workflow][WORKFLOW][PRICING][SUCCESS] ProductId: {productId} ⇢", productId);
                return "Pricing updated";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow][WORKFLOW][PRICING][ERROR] ProductId: {productId} ✖ ⇢", productId);
                throw;
            }
        }

        public async Task<string> UpdateInventory(Guid productId, InventoryDto dto)
        {
            _logger.LogInformation("[Workflow][WORKFLOW][INVENTORY][START] ProductId: {productId} User: {user} ⇢", productId, _httpContext.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value);
            try
            {
                var product = await _httpClient.GetFromJsonAsync<ProductDto>(
                    $"/api/products/{productId}"
                );

                var user = _httpContext.HttpContext?.User?
                    .FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (product.CreatedBy != user)
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][INVENTORY][FORBIDDEN] ProductId: {productId} User: {user} ⇢", productId, user);
                    throw new Exception("Only creator can update inventory");
                }

                if (product.Status != "Draft")
                {
                    _logger.LogWarning("[Workflow][WORKFLOW][INVENTORY][INVALID_STAGE] ProductId: {productId} Status: {status} ⇢", productId, product.Status);
                    throw new Exception("Can only update inventory in Draft state");
                }

                _logger.LogInformation("[Workflow][WORKFLOW][INVENTORY][SUCCESS] ProductId: {productId} ⇢", productId);
                return "Inventory updated";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow][WORKFLOW][INVENTORY][ERROR] ProductId: {productId} ✖ ⇢", productId);
                throw;
            }
        }
    }
}