using Microsoft.EntityFrameworkCore;
using WorkflowService.Entitites;
namespace WorkflowService.Data
{


    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Workflow> Workflows { get; set; }
    }
}
