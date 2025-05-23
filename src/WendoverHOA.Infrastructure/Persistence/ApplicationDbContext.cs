using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Infrastructure.Persistence
{
    /// <summary>
    /// The main database context for the Wendover HOA application
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class
        /// </summary>
        /// <param name="options">The database context options</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the refresh tokens
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Gets or sets the login attempts
        /// </summary>
        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        /// <summary>
        /// Gets or sets the user activities
        /// </summary>
        public DbSet<UserActivity> UserActivities { get; set; }

        /// <summary>
        /// Gets or sets the role permissions
        /// </summary>
        public DbSet<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Configures the database model
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity mappings
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure enum conversions
            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.Roles)
                .HasConversion(
                    v => string.Join(',', v.Select(r => r.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(r => Enum.Parse<UserRole>(r))
                          .ToList());

            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.Permissions)
                .HasConversion(
                    v => string.Join(',', v.Select(p => p.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(p => Enum.Parse<Permission>(p))
                          .ToList());
        }
    }
}
