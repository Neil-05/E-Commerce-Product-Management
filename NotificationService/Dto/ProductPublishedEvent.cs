using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Dto
{
    public class ProductPublishedEvent
    {
        public Guid ProductId { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
