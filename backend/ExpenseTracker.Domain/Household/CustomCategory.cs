using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Household;

/// <summary>A spending category the household added.</summary>
public sealed class CustomCategory
{
    private CustomCategory() { Name = string.Empty; }

    /// <summary>Creates a category with a tidy name.</summary>
    public CustomCategory(string? name)
    {
        var tidy = name?.Trim();
        if (!BillCategory.IsValid(tidy)) throw new DomainException($"Enter a category name of up to {BillCategory.MaxLength} characters.");
        Name = tidy!;
    }

    public string Name { get; private set; }
}
