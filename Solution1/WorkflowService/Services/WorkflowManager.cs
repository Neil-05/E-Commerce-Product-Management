using System.Net.Http.Json;
using WorkflowService.Data;
using WorkflowService.Dtos;
using WorkflowService.Entitites;

namespace WorkflowService.Services
{
    public class WorkflowManager : IWorkflowService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContext;
        private readonly HttpClient _httpClient;

        public WorkflowManager(
            AppDbContext context,
            IHttpContextAccessor httpContext,
            IHttpClientFactory factory)
        {
            _context = context;
            _httpContext = httpContext;
            _httpClient = factory.CreateClient();

            _httpClient.BaseAddress = new Uri("http://localhost:5191");
        }

        private string GetUser()
        {
            return _httpContext.HttpContext?.User?.Identity?.Name ?? "Unknown";
        }

        // 🔥 GET CURRENT STATUS FROM CATALOG
        private async Task<string> GetCurrentStatus(Guid productId)
        {
            var product = await _httpClient.GetFromJsonAsync<ProductDto>(
                $"/api/products/{productId}"
            );

            if (product == null)
                throw new Exception("Product not found");

            return product.Status;
        }

        // 🔥 SUBMIT (Draft → InReview)
        public async Task<string> Submit(Guid productId)
        {
            var currentStatus = await GetCurrentStatus(productId);

            if (currentStatus != "Draft")
                throw new Exception("Only Draft products can be submitted");

            await UpdateStatus(productId, "InReview");

            return "Submitted for review";
        }

        // 🔥 APPROVE (InReview → Approved)
        public async Task<string> Approve(Guid productId)
        {
            var currentStatus = await GetCurrentStatus(productId);

            if (currentStatus != "InReview")
                throw new Exception("Only InReview products can be approved");

            await UpdateStatus(productId, "Approved");

            return "Product Approved";
        }

        // 🔥 REJECT (InReview → Rejected)
        public async Task<string> Reject(Guid productId)
        {
            var currentStatus = await GetCurrentStatus(productId);

            if (currentStatus != "InReview")
                throw new Exception("Only InReview products can be rejected");

            await UpdateStatus(productId, "Rejected");

            return "Product Rejected";
        }

        // 🔥 COMMON STATUS UPDATE METHOD
        private async Task UpdateStatus(Guid productId, string status)
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
                throw new Exception("Catalog update failed");
        }
    }
}