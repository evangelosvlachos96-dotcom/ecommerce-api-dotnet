using EcommerceApp.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnType("uniqueidentifier")
                .HasConversion(s => new Guid(s), g => g.ToString("D"));
            builder.HasKey(e => e.Id).IsClustered(false);
            builder.Property(e => e.TotalPrice).IsRequired();
            builder.Property(e => e.Status).IsRequired();
            builder.Property(e => e.Invoice).IsRequired(false);
            builder.Property(e => e.CreatedAt).IsRequired();
            builder.Property(e => e.UpdatedAt).IsRequired();

            // Configure the many-to-many relationship
            builder.HasMany(o => o.Products)
                .WithMany(p => p.Orders)
                .UsingEntity(j => j.ToTable("OrderProducts"));
        }
    }
}