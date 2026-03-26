namespace CatalogService.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string SKU { get; set; }
        public Guid CategoryId { get; set; }
        public string Description { get; set; }

     
    }
}
