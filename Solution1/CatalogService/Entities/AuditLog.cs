using System.ComponentModel.DataAnnotations;

namespace CatalogService.Entities
{

    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Action { get; set; }
        public string EntityName { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public string CreatedBy { get; set; }   // ✅ NEW
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
