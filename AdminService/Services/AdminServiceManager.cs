using AdminService.Dto;
using System.Net.Http.Json;

namespace AdminService.Services
{
    public class AdminServiceManager : IAdminService
    {
        private readonly HttpClient _httpClient;


        public AdminServiceManager(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
        }

        // 🔥 DASHBOARD KPIs
        public async Task<object> GetDashboard()
        {
            var products = await _httpClient.GetFromJsonAsync<List<ProductDto>>(
                "http://localhost:5191/api/products"
            );

            return new
            {
                TotalProducts = products.Count,
                ApprovedProducts = products.Count(p => p.Status == "Approved"),
                PublishedProducts = products.Count(p => p.Status == "Published")
            };
        }

        // 🔥 AUDIT HISTORY
        public async Task<object> GetAudit(Guid productId)
        {
            // call Workflow Service
            var history = await _httpClient.GetFromJsonAsync<object>(
                $"http://localhost:5038/api/workflow/history/{productId}"
            );

            return history;
        }
    }
}