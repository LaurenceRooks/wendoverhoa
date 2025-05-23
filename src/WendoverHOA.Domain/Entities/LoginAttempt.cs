using WendoverHOA.Domain.Common;

namespace WendoverHOA.Domain.Entities;

/// <summary>
/// Represents a login attempt for security monitoring and auditing
/// </summary>
public class LoginAttempt : BaseEntity
{
    /// <summary>
    /// The username used in the login attempt
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// The user ID if the login was successful and the user exists
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Whether the login attempt was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// The IP address from which the login attempt was made
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// The user agent (browser/client) used for the login attempt
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;
    
    /// <summary>
    /// The date and time when the login attempt occurred
    /// </summary>
    public DateTime AttemptedAt { get; set; }
    
    /// <summary>
    /// The failure reason if the login attempt was unsuccessful
    /// </summary>
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// Navigation property for the associated user (if any)
    /// </summary>
    public virtual ApplicationUser? User { get; set; }
}
