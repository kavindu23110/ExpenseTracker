namespace ExpenseTracker.Domain.Household;

/// <summary>A line on the household's shopping list.</summary>
public sealed class ShoppingItem
{
    private ShoppingItem() { Key = Name = string.Empty; }

    /// <summary>Creates a validated line.</summary>
    public ShoppingItem(string key, string name)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name)) throw new DomainException("Enter an item name.");
        Key = key;
        Name = name.Trim();
    }

    public int Id { get; private set; }
    public string Key { get; private set; }
    public string Name { get; private set; }
    public bool Done { get; private set; }
    public decimal? Qty { get; private set; }
    public string? Unit { get; private set; }

    /// <summary>Sets how much to buy; no quantity means it is not known.</summary>
    public void SetAmount(decimal? qty, string? unit)
    {
        Qty = qty;
        Unit = unit;
    }

    /// <summary>Sets an amount entered by hand; a blank quantity clears the amount.</summary>
    public void ChangeAmount(decimal? qty, string? unit)
    {
        if (qty <= 0) throw new DomainException("Enter a quantity above zero, or leave it blank.");
        var trimmed = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        if (trimmed?.Length > 20) throw new DomainException("Keep the unit to 20 characters.");
        SetAmount(qty, qty is null ? null : trimmed);
    }

    /// <summary>Ticks or unticks the line.</summary>
    public void SetDone(bool done) => Done = done;
}
