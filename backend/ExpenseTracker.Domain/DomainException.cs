namespace ExpenseTracker.Domain;

/// <summary>Raised when a business rule rejects an operation.</summary>
public sealed class DomainException(string message) : Exception(message);
