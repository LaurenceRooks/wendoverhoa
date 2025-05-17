using WendoverHOA.Domain.Enums;

namespace WendoverHOA.Domain.Exceptions;

/// <summary>
/// Exception thrown when a user attempts an operation without sufficient permissions
/// </summary>
public class InsufficientPermissionException : DomainException
{
    public InsufficientPermissionException() 
        : base("The current user does not have sufficient permissions to perform this operation.") { }
    
    public InsufficientPermissionException(Permission requiredPermission) 
        : base($"The current user does not have the required permission: {requiredPermission}.") { }
    
    public InsufficientPermissionException(IEnumerable<Permission> requiredPermissions) 
        : base($"The current user does not have one or more of the required permissions: {string.Join(", ", requiredPermissions)}.") { }
    
    public InsufficientPermissionException(string message) 
        : base(message) { }
    
    public InsufficientPermissionException(string message, Exception innerException) 
        : base(message, innerException) { }
}
