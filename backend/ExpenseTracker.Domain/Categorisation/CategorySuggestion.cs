namespace ExpenseTracker.Domain.Categorisation;

/// <summary>A suggested category with how sure we are and why.</summary>
public sealed record CategorySuggestion(string Category, int Confidence, string Reason);
