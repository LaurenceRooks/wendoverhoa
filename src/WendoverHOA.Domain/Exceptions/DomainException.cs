namespace WendoverHOA.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-specific exceptions
/// </summary>
public class DomainException : Exception
{
    public DomainException() : base() { }
    
    public DomainException(string message) : base(message) { }
    
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}
