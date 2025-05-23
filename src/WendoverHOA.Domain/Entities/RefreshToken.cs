using WendoverHOA.Domain.Common;

namespace WendoverHOA.Domain.Entities;

/// <summary>
/// Represents a refresh token for JWT authentication
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// The user ID associated with this refresh token
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// The token value
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// The JWT ID associated with this refresh token
    /// </summary>
    public string JwtId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the token has been used
    /// </summary>
    public bool IsUsed { get; set; }
    
    /// <summary>
    /// Whether the token has been revoked
    /// </summary>
    public bool IsRevoked { get; set; }
    
    /// <summary>
    /// The date and time when the token was created
    /// </summary>
    public new DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// The date and time when the token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// The device/client information associated with this refresh token
    /// </summary>
    public string? DeviceInfo { get; set; }
    
    /// <summary>
    /// The IP address associated with this refresh token
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// Navigation property for the associated user
    /// </summary>
    public virtual ApplicationUser? User { get; set; }
    
    /// <summary>
    /// Checks if the refresh token is active (not used, not revoked, and not expired)
    /// </summary>
    /// <returns>True if the token is active, false otherwise</returns>
    public bool IsActive => !IsUsed && !IsRevoked && ExpiresAt > DateTime.UtcNow;
}
