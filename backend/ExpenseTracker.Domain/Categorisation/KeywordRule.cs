namespace ExpenseTracker.Domain.Categorisation;

/// <summary>English and Sinhala keywords for one category; Sinhala pairs carry an English gloss.</summary>
public sealed record KeywordRule(string Category, string[] Words, (string Word, string Gloss)[] Sinhala);
