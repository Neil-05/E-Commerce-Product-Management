namespace WorkflowService.Dtos
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string Status { get; set; }

        public string CreatedBy { get; set; } 
    }
}
