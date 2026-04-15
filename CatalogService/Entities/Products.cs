using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Entities;

public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(150)]
    public string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public string SKU { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    // 🔥 ADD THIS
    [MaxLength(200)]
    public string CreatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

  
}