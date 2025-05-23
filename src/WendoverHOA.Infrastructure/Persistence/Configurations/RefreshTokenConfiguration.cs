using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WendoverHOA.Domain.Entities;

namespace WendoverHOA.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the RefreshToken entity
    /// </summary>
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The entity type builder</param>
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            // Configure primary key
            builder.HasKey(t => t.Id);

            // Configure properties
            builder.Property(t => t.Token)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(t => t.JwtId)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.ExpiresAt)
                .IsRequired();

            builder.Property(t => t.DeviceInfo)
                .HasMaxLength(1000);

            builder.Property(t => t.IpAddress)
                .HasMaxLength(50);

            // Configure indexes
            builder.HasIndex(t => t.Token)
                .IsUnique();

            builder.HasIndex(t => t.JwtId);

            builder.HasIndex(t => t.UserId);

            // Configure relationships
            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
