namespace Moneta.Domain.Common;

/// <summary>
/// Raised when a business rule of the domain is violated.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
