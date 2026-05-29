using EcommerceApp.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Products Table
            builder.ToTable("Products");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Title).IsRequired();
            builder.Property(e => e.Price).IsRequired();
            builder.Property(e => e.Description).IsRequired();
            builder.Property(e => e.Category).IsRequired();
            builder.Property(e => e.Image).IsRequired();

            // Rating is an owned value object: Rate (double) and Count (int) are stored
            // as columns on the Products table. Both must be mapped and required.
            builder.OwnsOne(e => e.Rating, rating =>
                {
                    rating.Property(r => r.Rate).IsRequired();
                    rating.Property(r => r.Count).IsRequired();
                });
        }
    }
}
