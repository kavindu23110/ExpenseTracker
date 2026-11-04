namespace ExpenseTracker.Domain.PriceWatch;

/// <summary>A published market price an item can be compared with.</summary>
public sealed class MarketPrice
{
    private MarketPrice() { Key = string.Empty; Unit = string.Empty; Label = string.Empty; Description = string.Empty; }

    /// <summary>Creates a market reference.</summary>
    public MarketPrice(string key, decimal price, string unit, string label, string description)
    {
        Key = key;
        Price = price;
        Unit = unit;
        Label = label;
        Description = description;
    }

    public string Key { get; private set; }
    public decimal Price { get; private set; }
    public string Unit { get; private set; }
    public string Label { get; private set; }
    public string Description { get; private set; }
}
