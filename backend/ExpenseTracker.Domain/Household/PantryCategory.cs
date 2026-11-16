using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Household;

/// <summary>A spending category whose bought items are added to the pantry.</summary>
public sealed class PantryCategory
{
    private PantryCategory() { Name = string.Empty; }

    /// <summary>Marks a category as filling the pantry.</summary>
    public PantryCategory(string name)
    {
        if (!BillCategory.IsValid(name)) throw new DomainException("Choose a category.");
        Name = name.Trim();
    }

    public string Name { get; private set; }
}
