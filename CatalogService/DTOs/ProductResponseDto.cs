namespace CatalogService.DTOs
{
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string Status { get; set; }

        public string CreatedBy { get; set; }

        public List<string> Images { get; set; }

    }
}
