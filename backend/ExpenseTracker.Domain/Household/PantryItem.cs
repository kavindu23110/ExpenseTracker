namespace ExpenseTracker.Domain.Household;

/// <summary>Something the household has at home.</summary>
public sealed class PantryItem
{
    private PantryItem() { Key = Name = string.Empty; }

    /// <summary>Creates a validated pantry item.</summary>
    public PantryItem(string key, string name, decimal qty, string? unit)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name)) throw new DomainException("Enter an item name.");
        if (qty < 0) throw new DomainException("Quantity cannot be negative.");
        Key = key;
        Name = name.Trim();
        Qty = qty;
        Unit = unit;
    }

    public int Id { get; private set; }
    public string Key { get; private set; }
    public string Name { get; private set; }
    public decimal Qty { get; private set; }
    public string? Unit { get; private set; }

    /// <summary>True when none is left.</summary>
    public bool IsOut => Qty <= 0;

    /// <summary>Changes the quantity, stopping at zero.</summary>
    public void Adjust(decimal change) => Qty = Math.Max(0, Qty + change);

    /// <summary>Sets the quantity to an exact amount.</summary>
    public void SetQty(decimal qty)
    {
        if (qty < 0) throw new DomainException("Quantity cannot be negative.");
        Qty = qty;
    }

    /// <summary>Marks the item as finished.</summary>
    public void UseUp() => Qty = 0;
}
