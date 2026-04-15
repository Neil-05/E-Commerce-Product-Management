using Microsoft.EntityFrameworkCore;
using IdentityService.Entities;

namespace IdentityService.Data
{
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
}
}