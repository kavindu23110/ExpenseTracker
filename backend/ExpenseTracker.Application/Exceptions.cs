namespace ExpenseTracker.Application;

/// <summary>Raised when no bill reader is configured.</summary>
public sealed class BillReaderUnavailableException(string message) : Exception(message);

/// <summary>Raised when the bill reader could not produce a usable result.</summary>
public sealed class BillReadFailedException(string message, Exception? inner = null) : Exception(message, inner);
