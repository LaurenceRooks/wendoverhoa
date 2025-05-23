using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;
using WendoverHOA.Domain.Interfaces;
using WendoverHOA.Infrastructure.Identity;
using WendoverHOA.Infrastructure.Persistence;
using WendoverHOA.Infrastructure.Repositories;
using WendoverHOA.Infrastructure.Services;

namespace WendoverHOA.Infrastructure
{
    /// <summary>
    /// Extension methods for configuring dependency injection for the Infrastructure layer
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds infrastructure services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Configure Identity
            services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 10;
                    options.Password.RequiredUniqueChars = 5;

                    // Lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    // User settings
                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                    // Email confirmation settings
                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure JWT authentication
            var jwtSettings = configuration.GetSection("Jwt");
            var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var key = Encoding.UTF8.GetBytes(jwtKey);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers["Token-Expired"] = "true";
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // Configure authorization policies
            services.AddAuthorization(options =>
            {
                // Role-based policies
                options.AddPolicy("RequireAdministrator", policy => policy.RequireRole("Administrator"));
                options.AddPolicy("RequireBoardMember", policy => policy.RequireRole("Administrator", "BoardMember"));
                options.AddPolicy("RequireCommitteeMember", policy => policy.RequireRole("Administrator", "BoardMember", "CommitteeMember"));
                options.AddPolicy("RequireResident", policy => policy.RequireRole("Administrator", "BoardMember", "CommitteeMember", "Resident"));

                // Permission-based policies
                options.AddPolicy("CanManageUsers", policy => policy.RequireClaim("permission", "ManageUserRoles"));
                options.AddPolicy("CanCreateAnnouncements", policy => policy.RequireClaim("permission", "CreateAnnouncement"));
                options.AddPolicy("CanManageDocuments", policy => policy.RequireClaim("permission", "UploadDocument", "UpdateDocument", "DeleteDocument"));
            });

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();

            // Register services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IIdentityService, IdentityService>();

            return services;
        }

        /// <summary>
        /// Seeds the database with initial data
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

                await context.Database.MigrateAsync();
                await SeedRolesAsync(roleManager);
                await SeedAdminUserAsync(userManager);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
                logger.LogError(ex, "An error occurred while seeding the database");
            }
        }

        /// <summary>
        /// Seeds the roles in the database
        /// </summary>
        /// <param name="roleManager">The role manager</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            // Create roles if they don't exist
            var roles = new[] { "Administrator", "BoardMember", "CommitteeMember", "Resident", "Guest" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
        }

        /// <summary>
        /// Seeds the admin user in the database
        /// </summary>
        /// <param name="userManager">The user manager</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            // Create admin user if it doesn't exist
            var adminEmail = "admin@wendoverhoa.org";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false
                };

                // Add all roles and permissions to admin
                foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
                {
                    adminUser.AddRole(role);
                }

                foreach (Permission permission in Enum.GetValues(typeof(Permission)))
                {
                    adminUser.AddPermission(permission);
                }

                var result = await userManager.CreateAsync(adminUser, "Admin@WendoverHOA123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
            }
        }
    }
}
