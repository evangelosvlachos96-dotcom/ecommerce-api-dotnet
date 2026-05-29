using EcommerceApp.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceApp.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Users Table
            builder.ToTable("Users");

            builder.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnType("uniqueidentifier")
                .HasConversion(s => new Guid(s), g => g.ToString("D"));
            builder.HasKey(e => e.Id).IsClustered(false);
            builder.Property(e => e.FirstName).IsRequired().HasMaxLength(36);
            builder.Property(e => e.LastName).IsRequired().HasMaxLength(36);
            builder.Property(e => e.Email).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Password).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Role).IsRequired().HasMaxLength(36);
        }
    }
}
