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

    public const int MaxLocationLength = 40;

    /// <summary>When it goes off; null when not tracked.</summary>
    public DateOnly? Expiry { get; private set; }

    /// <summary>Where it is kept, such as Fridge or Freezer.</summary>
    public string? Location { get; private set; }

    /// <summary>The quantity at or below which it should be bought again.</summary>
    public decimal? LowAt { get; private set; }

    /// <summary>True when some is left but no more than the low level.</summary>
    public bool IsLow => !IsOut && LowAt is { } low && Qty <= low;

    /// <summary>True when some is left and it expires on or before the given day.</summary>
    public bool ExpiresBy(DateOnly day) => !IsOut && Expiry is { } e && e <= day;

    /// <summary>Changes the expiry, place and low level; blanks clear them.</summary>
    public void SetDetails(DateOnly? expiry, string? location, decimal? lowAt)
    {
        var place = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        if (place is { Length: > MaxLocationLength }) throw new DomainException($"Use a place name of up to {MaxLocationLength} characters.");
        if (lowAt < 0) throw new DomainException("The low level cannot be negative.");
        Expiry = expiry;
        Location = place;
        LowAt = lowAt is 0 ? null : lowAt;
    }

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
