using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MediaAsset> MediaAssets { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
