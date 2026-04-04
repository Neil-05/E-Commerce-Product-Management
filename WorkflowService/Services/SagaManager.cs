using WorkflowService.Data;
using WorkflowService.Entities;

namespace WorkflowService.Services
{
    public interface ISagaManager
    {
        Task<Guid> StartSaga(Guid productId);
        Task CompleteSaga(Guid sagaId);
        Task CancelSaga(Guid sagaId);
    }

    public class SagaManager : ISagaManager
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SagaManager> _logger;

        public SagaManager(AppDbContext context, ILogger<SagaManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid> StartSaga(Guid productId)
        {
            var saga = new Saga
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Status = "Started",
                CreatedAt = DateTime.UtcNow
            };

            _context.Sagas.Add(saga);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[SAGA][START] SagaId: {sagaId} ProductId: {productId}", saga.Id, productId);
            return saga.Id;
        }

        public async Task CompleteSaga(Guid sagaId)
        {
            var saga = await _context.Sagas.FindAsync(sagaId);
            if (saga == null) throw new Exception("Saga not found");

            saga.Status = "Completed";
            saga.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("[SAGA][COMPLETE] SagaId: {sagaId}", sagaId);
        }

        public async Task CancelSaga(Guid sagaId)
        {
            var saga = await _context.Sagas.FindAsync(sagaId);
            if (saga == null) throw new Exception("Saga not found");

            saga.Status = "Cancelled";
            saga.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("[SAGA][CANCEL] SagaId: {sagaId}", sagaId);
        }
    }
}
