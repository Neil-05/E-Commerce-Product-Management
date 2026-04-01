using WorkflowService.Dtos;

namespace WorkflowService.Services
{
    public interface IWorkflowService
    {
        Task<string> Submit(Guid productId);
        Task<string> Approve(Guid productId);
        Task<string> Reject(Guid productId);

        Task<string> Publish(Guid productId);


        Task<List<WorkflowHistoryDto>> GetHistory(Guid productId);
        Task<string> UpdatePricing(Guid productId, PricingDto dto);
        Task<string> UpdateInventory(Guid productId, InventoryDto dto);
    }
}
