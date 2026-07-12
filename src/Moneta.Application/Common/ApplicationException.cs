namespace Moneta.Application.Common;

/// <summary>Thrown when a requested resource does not exist.</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>Thrown when a request conflicts with the current state (e.g. duplicate).</summary>
public sealed class ConflictException(string message) : Exception(message);

/// <summary>Thrown when authentication fails.</summary>
public sealed class UnauthorizedException(string message) : Exception(message);
