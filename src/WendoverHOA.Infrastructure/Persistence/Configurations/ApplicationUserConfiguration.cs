using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the ApplicationUser entity
    /// </summary>
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The entity type builder</param>
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("AspNetUsers");

            // Configure properties
            builder.Property(u => u.UserName)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(u => u.ProfilePictureUrl)
                .HasMaxLength(1000);

            // Configure indexes
            builder.HasIndex(u => u.UserName)
                .IsUnique();

            builder.HasIndex(u => u.Email)
                .IsUnique();

            // Configure enum collections as JSON
            builder.Property(u => u.Roles)
                .HasConversion(
                    v => string.Join(',', v.Select(r => (int)r)),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => (UserRole)int.Parse(r))
                        .ToList())
                .HasColumnType("nvarchar(max)");

            builder.Property(u => u.Permissions)
                .HasConversion(
                    v => string.Join(',', v.Select(p => (int)p)),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => (Permission)int.Parse(p))
                        .ToList())
                .HasColumnType("nvarchar(max)");
        }
    }
}
