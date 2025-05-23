using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WendoverHOA.Domain.Entities;
using WendoverHOA.Domain.Interfaces;
using WendoverHOA.Infrastructure.Persistence;

namespace WendoverHOA.Infrastructure.Services
{
    /// <summary>
    /// Service for JWT token generation and validation
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class
        /// </summary>
        /// <param name="configuration">The configuration</param>
        /// <param name="userManager">The user manager</param>
        /// <param name="context">The database context</param>
        public TokenService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>The JWT token</returns>
        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };
            
            // Add roles as claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            
            // Add custom permissions as claims
            claims.AddRange(user.Permissions.Select(permission => 
                new Claim("permission", permission.ToString())));

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // Short-lived access token
                signingCredentials: creds);
                
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a refresh token for the specified user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="jwtId">The JWT ID</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="deviceInfo">The device information</param>
        /// <returns>The refresh token</returns>
        public async Task<RefreshToken> GenerateRefreshTokenAsync(
            int userId, 
            string jwtId, 
            string ipAddress, 
            string deviceInfo)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = GenerateRefreshTokenString(),
                JwtId = jwtId,
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14), // 14-day refresh token
                IpAddress = ipAddress,
                DeviceInfo = deviceInfo
            };
            
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            
            return refreshToken;
        }

        /// <summary>
        /// Validates a refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="userId">The user ID</param>
        /// <returns>The validation result</returns>
        public async Task<(bool IsValid, string? ErrorMessage, RefreshToken? RefreshToken)> ValidateRefreshTokenAsync(
            string refreshToken, 
            int userId)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
                
            if (storedToken == null)
            {
                return (false, "Invalid refresh token", null);
            }
            
            if (storedToken.IsUsed)
            {
                return (false, "Refresh token has been used", null);
            }
            
            if (storedToken.IsRevoked)
            {
                return (false, "Refresh token has been revoked", null);
            }
            
            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return (false, "Refresh token has expired", null);
            }
            
            return (true, null, storedToken);
        }

        /// <summary>
        /// Marks a refresh token as used
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        public async Task UseRefreshTokenAsync(RefreshToken refreshToken)
        {
            refreshToken.IsUsed = true;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="reason">The reason for revocation</param>
        public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? reason = null)
        {
            refreshToken.IsRevoked = true;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
            
            // Log the revocation
            var userActivity = new UserActivity
            {
                UserId = refreshToken.UserId,
                ActivityType = "RefreshTokenRevoked",
                OccurredAt = DateTime.UtcNow,
                IpAddress = refreshToken.IpAddress ?? "Unknown",
                UserAgent = refreshToken.DeviceInfo ?? "Unknown",
                Details = reason
            };
            
            await _context.UserActivities.AddAsync(userActivity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Revokes all refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="reason">The reason for revocation</param>
        public async Task RevokeAllRefreshTokensAsync(int userId, string? reason = null)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsUsed && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
                
            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
            }
            
            await _context.SaveChangesAsync();
            
            // Log the revocation
            var userActivity = new UserActivity
            {
                UserId = userId,
                ActivityType = "AllRefreshTokensRevoked",
                OccurredAt = DateTime.UtcNow,
                IpAddress = "System",
                UserAgent = "System",
                Details = reason
            };
            
            await _context.UserActivities.AddAsync(userActivity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Generates a random refresh token string
        /// </summary>
        /// <returns>The refresh token string</returns>
        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    /// <summary>
    /// Interface for the token service
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>The JWT token</returns>
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
        
        /// <summary>
        /// Generates a refresh token for the specified user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="jwtId">The JWT ID</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="deviceInfo">The device information</param>
        /// <returns>The refresh token</returns>
        Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string jwtId, string ipAddress, string deviceInfo);
        
        /// <summary>
        /// Validates a refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="userId">The user ID</param>
        /// <returns>The validation result</returns>
        Task<(bool IsValid, string? ErrorMessage, RefreshToken? RefreshToken)> ValidateRefreshTokenAsync(string refreshToken, int userId);
        
        /// <summary>
        /// Marks a refresh token as used
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        Task UseRefreshTokenAsync(RefreshToken refreshToken);
        
        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="reason">The reason for revocation</param>
        Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? reason = null);
        
        /// <summary>
        /// Revokes all refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="reason">The reason for revocation</param>
        Task RevokeAllRefreshTokensAsync(int userId, string? reason = null);
    }
}
