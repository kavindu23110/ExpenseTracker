namespace ExpenseTracker.Domain.Bills;

/// <summary>A category the user picked for an item, remembered for later bills.</summary>
public sealed class ItemCategoryRule
{
    private ItemCategoryRule() { Key = Category = string.Empty; }

    /// <summary>Remembers a category for an item key.</summary>
    public ItemCategoryRule(string key, string category)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new DomainException("An item is needed.");
        if (!BillCategory.IsValid(category)) throw new DomainException("Choose a category.");
        Key = key;
        Category = category;
    }

    public string Key { get; private set; }
    public string Category { get; private set; }

    /// <summary>Changes the remembered category.</summary>
    public void ChangeTo(string category)
    {
        if (!BillCategory.IsValid(category)) throw new DomainException("Choose a category.");
        Category = category;
    }
}
