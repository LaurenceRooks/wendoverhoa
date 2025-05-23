using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WendoverHOA.Domain.Entities;

namespace WendoverHOA.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the UserActivity entity
    /// </summary>
    public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
    {
        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The entity type builder</param>
        public void Configure(EntityTypeBuilder<UserActivity> builder)
        {
            builder.ToTable("UserActivities");

            // Configure primary key
            builder.HasKey(ua => ua.Id);

            // Configure properties
            builder.Property(ua => ua.ActivityType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(ua => ua.OccurredAt)
                .IsRequired();

            builder.Property(ua => ua.IpAddress)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(ua => ua.UserAgent)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(ua => ua.Details)
                .HasMaxLength(4000);

            builder.Property(ua => ua.Area)
                .HasMaxLength(100);

            builder.Property(ua => ua.Controller)
                .HasMaxLength(100);

            builder.Property(ua => ua.Action)
                .HasMaxLength(100);

            // Configure indexes
            builder.HasIndex(ua => ua.UserId);
            
            builder.HasIndex(ua => ua.ActivityType);
            
            builder.HasIndex(ua => ua.OccurredAt);

            // Configure relationships
            builder.HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
