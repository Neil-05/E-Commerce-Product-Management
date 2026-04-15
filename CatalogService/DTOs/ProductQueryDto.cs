namespace CatalogService.DTOs
{
    public class ProductQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
