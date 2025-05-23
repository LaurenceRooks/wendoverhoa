using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WendoverHOA.Domain.Entities;

namespace WendoverHOA.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the LoginAttempt entity
    /// </summary>
    public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
    {
        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The entity type builder</param>
        public void Configure(EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.ToTable("LoginAttempts");

            // Configure primary key
            builder.HasKey(la => la.Id);

            // Configure properties
            builder.Property(la => la.Username)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(la => la.IpAddress)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(la => la.UserAgent)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(la => la.AttemptedAt)
                .IsRequired();

            builder.Property(la => la.FailureReason)
                .HasMaxLength(1000);

            // Configure indexes
            builder.HasIndex(la => la.Username);
            
            builder.HasIndex(la => la.UserId);
            
            builder.HasIndex(la => la.AttemptedAt);
            
            builder.HasIndex(la => la.IpAddress);

            // Configure relationships
            builder.HasOne(la => la.User)
                .WithMany()
                .HasForeignKey(la => la.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
