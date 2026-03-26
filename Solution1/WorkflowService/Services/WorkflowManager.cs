using WorkflowService.Data;
using WorkflowService.Entitites;
using System.Net.Http.Json;

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

            // ✅ Catalog Service URL (CHECK YOUR PORT)
            _httpClient.BaseAddress = new Uri("http://localhost:5191");
        }

        private string GetUser()
        {
            return _httpContext.HttpContext?.User?.Identity?.Name ?? "Unknown";
        }

        // 🔥 SUBMIT
        public async Task<string> Submit(Guid productId)
        {
            var workflow = new Workflow
            {
                ProductId = productId,
                Status = "InReview",
                ActionBy = GetUser(),
                Timestamp = DateTime.UtcNow
            };

            _context.Workflows.Add(workflow);
            await _context.SaveChangesAsync();

            // ✅ FIXED ENDPOINT
            var response = await _httpClient.PutAsJsonAsync(
                $"/api/products/{productId}/status",
                new { status = "InReview" }
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception("Catalog update failed");

            return "Submitted for review";
        }

        // 🔥 APPROVE
        public async Task<string> Approve(Guid productId)
        {
            var workflow = new Workflow
            {
                ProductId = productId,
                Status = "Approved",
                ActionBy = GetUser(),
                Timestamp = DateTime.UtcNow
            };

            _context.Workflows.Add(workflow);
            await _context.SaveChangesAsync();

            var response = await _httpClient.PutAsJsonAsync(
                $"/api/products/{productId}/status",
                new { status = "Approved" }
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception("Catalog update failed");

            return "Product Approved";
        }

        // 🔥 REJECT
        public async Task<string> Reject(Guid productId)
        {
            var workflow = new Workflow
            {
                ProductId = productId,
                Status = "Rejected",
                ActionBy = GetUser(),
                Timestamp = DateTime.UtcNow
            };

            _context.Workflows.Add(workflow);
            await _context.SaveChangesAsync();

            var response = await _httpClient.PutAsJsonAsync(
                $"/api/products/{productId}/status",
                new { status = "Rejected" }
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception("Catalog update failed");

            return "Product Rejected";
        }
    }
}