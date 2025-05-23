using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Enums;
using WendoverHOA.Infrastructure.Persistence;
using WendoverHOA.Infrastructure.Services;

namespace WendoverHOA.Infrastructure.Identity
{
    /// <summary>
    /// Service for identity operations
    /// </summary>
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityService"/> class
        /// </summary>
        /// <param name="userManager">The user manager</param>
        /// <param name="signInManager">The sign-in manager</param>
        /// <param name="roleManager">The role manager</param>
        /// <param name="tokenService">The token service</param>
        /// <param name="context">The database context</param>
        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            ITokenService tokenService,
            ApplicationDbContext context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="email">The email</param>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="firstName">The first name</param>
        /// <param name="lastName">The last name</param>
        /// <returns>The result of the registration</returns>
        public async Task<(bool Succeeded, string[] Errors, ApplicationUser? User)> RegisterUserAsync(
            string email,
            string username,
            string password,
            string firstName,
            string lastName)
        {
            // Check if email is already registered
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return (false, new[] { "Email is already registered" }, null);
            }

            // Check if username is already taken
            existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null)
            {
                return (false, new[] { "Username is already taken" }, null);
            }

            // Create the new user
            var user = new ApplicationUser
            {
                Email = email,
                UserName = username,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0
            };

            // Add the Resident role by default
            user.AddRole(UserRole.Resident);

            // Add default permissions for residents
            user.AddPermission(Permission.ViewPublicContent);
            user.AddPermission(Permission.ViewAnnouncements);
            user.AddPermission(Permission.ViewCalendar);
            user.AddPermission(Permission.ViewDocuments);
            user.AddPermission(Permission.ViewDirectory);

            // Create the user with password
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray(), null);
            }

            // Add to Resident role
            await _userManager.AddToRoleAsync(user, UserRole.Resident.ToString());

            // Log the registration
            var userActivity = new UserActivity
            {
                UserId = user.Id,
                ActivityType = "Registration",
                OccurredAt = DateTime.UtcNow,
                IpAddress = "System",
                UserAgent = "System"
            };

            await _context.UserActivities.AddAsync(userActivity);
            await _context.SaveChangesAsync();

            return (true, Array.Empty<string>(), user);
        }

        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="usernameOrEmail">The username or email</param>
        /// <param name="password">The password</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="userAgent">The user agent</param>
        /// <returns>The result of the authentication</returns>
        public async Task<(bool Succeeded, string[] Errors, ApplicationUser? User)> AuthenticateAsync(
            string usernameOrEmail,
            string password,
            string ipAddress,
            string userAgent)
        {
            // Find the user by username or email
            var user = await _userManager.FindByNameAsync(usernameOrEmail) ??
                      await _userManager.FindByEmailAsync(usernameOrEmail);

            if (user == null)
            {
                // Log failed login attempt
                await LogLoginAttemptAsync(usernameOrEmail, null, false, ipAddress, userAgent, "User not found");
                return (false, new[] { "Invalid credentials" }, null);
            }

            // Check if the user is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                // Log failed login attempt
                await LogLoginAttemptAsync(usernameOrEmail, user.Id, false, ipAddress, userAgent, "Account locked out");
                return (false, new[] { "Account is locked out" }, null);
            }

            // Verify the password
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);

            if (!result.Succeeded)
            {
                string failureReason = "Invalid password";
                if (result.IsLockedOut)
                {
                    failureReason = "Account locked out";
                }
                else if (result.IsNotAllowed)
                {
                    failureReason = "Account not allowed to sign in";
                }
                else if (result.RequiresTwoFactor)
                {
                    failureReason = "Two-factor authentication required";
                }

                // Log failed login attempt
                await LogLoginAttemptAsync(usernameOrEmail, user.Id, false, ipAddress, userAgent, failureReason);

                return (false, new[] { failureReason }, null);
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Log successful login attempt
            await LogLoginAttemptAsync(usernameOrEmail, user.Id, true, ipAddress, userAgent, null);

            return (true, Array.Empty<string>(), user);
        }

        /// <summary>
        /// Generates authentication tokens for a user
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="deviceInfo">The device information</param>
        /// <returns>The authentication tokens</returns>
        public async Task<(string AccessToken, string RefreshToken)> GenerateAuthTokensAsync(
            ApplicationUser user,
            string ipAddress,
            string deviceInfo)
        {
            // Generate JWT access token
            var accessToken = await _tokenService.GenerateJwtTokenAsync(user);

            // Extract JWT ID from the token
            var jwtId = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Id;

            // Generate refresh token
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, jwtId, ipAddress, deviceInfo);

            return (accessToken, refreshToken.Token);
        }

        /// <summary>
        /// Refreshes authentication tokens
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="userId">The user ID</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="deviceInfo">The device information</param>
        /// <returns>The new authentication tokens</returns>
        public async Task<(bool Succeeded, string[] Errors, string? AccessToken, string? RefreshToken)> RefreshTokensAsync(
            string refreshToken,
            int userId,
            string ipAddress,
            string deviceInfo)
        {
            // Validate the refresh token
            var validationResult = await _tokenService.ValidateRefreshTokenAsync(refreshToken, userId);

            if (!validationResult.IsValid)
            {
                return (false, new[] { validationResult.ErrorMessage ?? "Invalid refresh token" }, null, null);
            }

            // Get the user
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return (false, new[] { "User not found" }, null, null);
            }

            // Mark the old refresh token as used
            await _tokenService.UseRefreshTokenAsync(validationResult.RefreshToken!);

            // Generate new tokens
            var (accessToken, newRefreshToken) = await GenerateAuthTokensAsync(user, ipAddress, deviceInfo);

            // Log token refresh
            var userActivity = new UserActivity
            {
                UserId = userId,
                ActivityType = "TokenRefresh",
                OccurredAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = deviceInfo
            };

            await _context.UserActivities.AddAsync(userActivity);
            await _context.SaveChangesAsync();

            return (true, Array.Empty<string>(), accessToken, newRefreshToken);
        }

        /// <summary>
        /// Logs out a user by revoking their refresh tokens
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="refreshToken">The refresh token to revoke (null to revoke all)</param>
        /// <returns>The result of the logout operation</returns>
        public async Task<bool> LogoutAsync(int userId, string? refreshToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    // Revoke all refresh tokens for the user
                    await _tokenService.RevokeAllRefreshTokensAsync(userId, "User logout");
                }
                else
                {
                    // Validate and revoke the specific refresh token
                    var validationResult = await _tokenService.ValidateRefreshTokenAsync(refreshToken, userId);
                    if (validationResult.IsValid && validationResult.RefreshToken != null)
                    {
                        await _tokenService.RevokeRefreshTokenAsync(validationResult.RefreshToken, "User logout");
                    }
                }

                // Log the logout
                var userActivity = new UserActivity
                {
                    UserId = userId,
                    ActivityType = "Logout",
                    OccurredAt = DateTime.UtcNow,
                    IpAddress = "System",
                    UserAgent = "System"
                };

                await _context.UserActivities.AddAsync(userActivity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a user to a role
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="role">The role</param>
        /// <returns>The result of the operation</returns>
        public async Task<(bool Succeeded, string[] Errors)> AddUserToRoleAsync(int userId, UserRole role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return (false, new[] { "User not found" });
            }

            // Ensure the role exists
            var roleName = role.ToString();
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }

            // Add the user to the role
            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            // Update the user's Roles collection
            user.AddRole(role);
            await _userManager.UpdateAsync(user);

            return (true, Array.Empty<string>());
        }

        /// <summary>
        /// Removes a user from a role
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="role">The role</param>
        /// <returns>The result of the operation</returns>
        public async Task<(bool Succeeded, string[] Errors)> RemoveUserFromRoleAsync(int userId, UserRole role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return (false, new[] { "User not found" });
            }

            // Remove the user from the role
            var result = await _userManager.RemoveFromRoleAsync(user, role.ToString());

            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description).ToArray());
            }

            // Update the user's Roles collection
            user.RemoveRole(role);
            await _userManager.UpdateAsync(user);

            return (true, Array.Empty<string>());
        }

        /// <summary>
        /// Gets the roles for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The roles</returns>
        public async Task<IList<string>> GetUserRolesAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new List<string>();
            }

            return await _userManager.GetRolesAsync(user);
        }

        /// <summary>
        /// Checks if a user is in a role
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="role">The role</param>
        /// <returns>True if the user is in the role, false otherwise</returns>
        public async Task<bool> IsUserInRoleAsync(int userId, UserRole role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, role.ToString());
        }

        /// <summary>
        /// Logs a login attempt
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="userId">The user ID</param>
        /// <param name="isSuccessful">Whether the login was successful</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="userAgent">The user agent</param>
        /// <param name="failureReason">The failure reason</param>
        private async Task LogLoginAttemptAsync(
            string username,
            int? userId,
            bool isSuccessful,
            string ipAddress,
            string userAgent,
            string? failureReason)
        {
            var loginAttempt = new LoginAttempt
            {
                Username = username,
                UserId = userId,
                IsSuccessful = isSuccessful,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                AttemptedAt = DateTime.UtcNow,
                FailureReason = failureReason
            };

            await _context.LoginAttempts.AddAsync(loginAttempt);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Interface for the identity service
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="email">The email</param>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="firstName">The first name</param>
        /// <param name="lastName">The last name</param>
        /// <returns>The result of the registration</returns>
        Task<(bool Succeeded, string[] Errors, ApplicationUser? User)> RegisterUserAsync(
            string email,
            string username,
            string password,
            string firstName,
            string lastName);

        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="usernameOrEmail">The username or email</param>
        /// <param name="password">The password</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="userAgent">The user agent</param>
        /// <returns>The result of the authentication</returns>
        Task<(bool Succeeded, string[] Errors, ApplicationUser? User)> AuthenticateAsync(
            string usernameOrEmail,
            string password,
            string ipAddress,
            string userAgent);

        /// <summary>
        /// Generates authentication tokens for a user
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="deviceInfo">The device information</param>
        /// <returns>The authentication tokens</returns>
        Task<(string AccessToken, string RefreshToken)> GenerateAuthTokensAsync(
            ApplicationUser user,
            string ipAddress,
            string deviceInfo);

        /// <summary>
        /// Refreshes authentication tokens
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="userId">The user ID</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="deviceInfo">The device information</param>
        /// <returns>The new authentication tokens</returns>
        Task<(bool Succeeded, string[] Errors, string? AccessToken, string? RefreshToken)> RefreshTokensAsync(
            string refreshToken,
            int userId,
            string ipAddress,
            string deviceInfo);

        /// <summary>
        /// Logs out a user by revoking their refresh tokens
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="refreshToken">The refresh token to revoke (null to revoke all)</param>
        /// <returns>The result of the logout operation</returns>
        Task<bool> LogoutAsync(int userId, string? refreshToken = null);

        /// <summary>
        /// Adds a user to a role
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="role">The role</param>
        /// <returns>The result of the operation</returns>
        Task<(bool Succeeded, string[] Errors)> AddUserToRoleAsync(int userId, UserRole role);

        /// <summary>
        /// Removes a user from a role
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="role">The role</param>
        /// <returns>The result of the operation</returns>
        Task<(bool Succeeded, string[] Errors)> RemoveUserFromRoleAsync(int userId, UserRole role);

        /// <summary>
        /// Gets the roles for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The roles</returns>
        Task<IList<string>> GetUserRolesAsync(int userId);

        /// <summary>
        /// Checks if a user is in a role
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="role">The role</param>
        /// <returns>True if the user is in the role, false otherwise</returns>
        Task<bool> IsUserInRoleAsync(int userId, UserRole role);
    }
}
