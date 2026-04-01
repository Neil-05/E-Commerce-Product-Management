using System.ComponentModel.DataAnnotations;

namespace WorkflowService.Entitites
{
   
    public class Workflow
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProductId { get; set; }

        public string Status { get; set; } // Draft, InReview, Approved, Rejected

        public string ActionBy { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
