namespace WorkflowService.Services
{
    public interface IWorkflowService
    {
        Task<string> Submit(Guid productId);
        Task<string> Approve(Guid productId);
        Task<string> Reject(Guid productId);
    }
}
