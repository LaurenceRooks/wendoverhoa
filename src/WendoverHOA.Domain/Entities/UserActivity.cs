using WendoverHOA.Domain.Common;

namespace WendoverHOA.Domain.Entities;

/// <summary>
/// Represents a user activity for audit purposes
/// </summary>
public class UserActivity : BaseEntity
{
    /// <summary>
    /// The user ID associated with this activity
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// The type of activity
    /// </summary>
    public string ActivityType { get; set; } = string.Empty;
    
    /// <summary>
    /// The date and time when the activity occurred
    /// </summary>
    public DateTime OccurredAt { get; set; }
    
    /// <summary>
    /// The IP address from which the activity originated
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// The user agent (browser/client) used for the activity
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional details about the activity (JSON serialized)
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// The area of the application where the activity occurred
    /// </summary>
    public string? Area { get; set; }
    
    /// <summary>
    /// The controller where the activity occurred
    /// </summary>
    public string? Controller { get; set; }
    
    /// <summary>
    /// The action where the activity occurred
    /// </summary>
    public string? Action { get; set; }
    
    /// <summary>
    /// Navigation property for the associated user
    /// </summary>
    public virtual ApplicationUser? User { get; set; }
}
