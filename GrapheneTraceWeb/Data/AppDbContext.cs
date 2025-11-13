using Microsoft.EntityFrameworkCore;
using GrapheneTraceWeb.Models;

namespace GrapheneTraceWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PressureData> PressureData { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}

