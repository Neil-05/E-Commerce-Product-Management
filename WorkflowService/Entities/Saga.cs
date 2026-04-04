using System;

namespace WorkflowService.Entities
{
    public class Saga
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
