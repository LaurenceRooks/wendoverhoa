namespace WendoverHOA.Domain.Enums;

/// <summary>
/// Standardized user roles for the Wendover HOA application
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Unauthenticated users with limited access to public information
    /// </summary>
    Guest = 0,
    
    /// <summary>
    /// Authenticated homeowners with access to community information and personal data
    /// </summary>
    Resident = 1,
    
    /// <summary>
    /// Residents with additional permissions for specific committees
    /// </summary>
    CommitteeMember = 2,
    
    /// <summary>
    /// HOA board members with administrative access for managing community content and operations
    /// </summary>
    BoardMember = 3,
    
    /// <summary>
    /// Full system access for managing all aspects of the application
    /// </summary>
    Administrator = 4
}
