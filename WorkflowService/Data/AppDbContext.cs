using Microsoft.EntityFrameworkCore;
using WorkflowService.Entities;
using WorkflowService.Entities;
namespace WorkflowService.Data
{


    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<Saga> Sagas { get; set; }
    }
}
