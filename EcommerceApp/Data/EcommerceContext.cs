using EcommerceApp.Data.Configurations;
using EcommerceApp.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Data
{
    public class EcommerceContext : DbContext
    {
        // dotnet ef migrations add {MigrationName} --context EcommerceContext
        // dotnet ef database update --context EcommerceContext
        public EcommerceContext(DbContextOptions<EcommerceContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
        }
    }
}
