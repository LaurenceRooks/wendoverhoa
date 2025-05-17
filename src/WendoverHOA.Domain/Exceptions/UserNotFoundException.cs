namespace WendoverHOA.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested user cannot be found
/// </summary>
public class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid userId) 
        : base($"User with ID {userId} was not found.") { }
    
    public UserNotFoundException(string username) 
        : base($"User with username '{username}' was not found.") { }
    
    public UserNotFoundException(string message, Exception innerException) 
        : base(message, innerException) { }
}
