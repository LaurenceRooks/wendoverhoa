namespace WendoverHOA.Domain.ValueObjects;

/// <summary>
/// Represents a physical address as a value object
/// </summary>
public class Address : IEquatable<Address>
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string? Unit { get; private set; }

    // Private constructor to enforce creation through factory method
    private Address() { }

    public Address(string street, string city, string state, string zipCode, string? unit = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));
        
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode cannot be empty", nameof(zipCode));

        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Unit = unit;
    }

    public static Address Create(string street, string city, string state, string zipCode, string? unit = null)
    {
        return new Address(street, city, state, zipCode, unit);
    }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Unit)
            ? $"{Street}, {City}, {State} {ZipCode}"
            : $"{Street} Unit {Unit}, {City}, {State} {ZipCode}";
    }

    public override bool Equals(object? obj)
    {
        return obj is Address address && Equals(address);
    }

    public bool Equals(Address? other)
    {
        if (other is null)
            return false;

        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               ZipCode == other.ZipCode &&
               Unit == other.Unit;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Street, City, State, ZipCode, Unit);
    }

    public static bool operator ==(Address? left, Address? right)
    {
        if (left is null)
            return right is null;
        
        return left.Equals(right);
    }

    public static bool operator !=(Address? left, Address? right)
    {
        return !(left == right);
    }
}
