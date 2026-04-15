namespace AdminService.Services
{
    public interface IAdminService
    {
        Task<object> GetDashboard();
        Task<object> GetAudit(Guid productId);
    }
}
