namespace ExpenseTracker.Domain.Bills;

/// <summary>The built-in spending categories; the household can add more.</summary>
public static class BillCategory
{
    public const string Groceries = "Groceries";
    public const string Dining = "Dining";
    public const string Transport = "Transport";
    public const string Utilities = "Utilities";
    public const string Shopping = "Shopping";
    public const string Health = "Health";
    public const string CreditCard = "Credit card";
    public const string Other = "Other";
    public const int MaxLength = 50;

    public static readonly IReadOnlyList<string> All = [Groceries, Dining, Transport, Utilities, Shopping, Health, CreditCard, Other];

    /// <summary>True for a usable category name; services check it exists.</summary>
    public static bool IsValid(string? category) => !string.IsNullOrWhiteSpace(category) && category.Length <= MaxLength;
}
