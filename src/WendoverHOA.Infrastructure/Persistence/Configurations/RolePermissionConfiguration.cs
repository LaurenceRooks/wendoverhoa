using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the RolePermission entity
    /// </summary>
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The entity type builder</param>
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");

            // Configure primary key
            builder.HasKey(rp => rp.Id);

            // Configure properties
            builder.Property(rp => rp.RoleName)
                .HasMaxLength(256)
                .IsRequired();

            // Configure enum properties with conversions
            builder.Property(rp => rp.Role)
                .HasConversion<string>();

            builder.Property(rp => rp.Permission)
                .HasConversion<string>();

            // Configure indexes
            builder.HasIndex(rp => new { rp.RoleName, rp.Permission })
                .IsUnique();
        }
    }
}
