namespace ExpenseTracker.Domain.Bills;

/// <summary>One line on a bill, with its optional marked price and market reference.</summary>
public sealed class BillItem
{
    private BillItem() { Name = string.Empty; }

    /// <summary>Creates a validated line item.</summary>
    public BillItem(string name, decimal qty, decimal price, string? unit = null, string? nameSi = null, decimal? markedPrice = null, string? marketKey = null)
    {
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(nameSi)) throw new DomainException("A line item needs a name.");
        if (qty <= 0) throw new DomainException($"Quantity for '{name}' must be above zero.");
        if (price < 0) throw new DomainException($"Price for '{name}' cannot be negative.");
        Name = string.IsNullOrWhiteSpace(name) ? nameSi!.Trim() : name.Trim();
        Qty = qty;
        Price = price;
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        NameSi = string.IsNullOrWhiteSpace(nameSi) ? null : nameSi.Trim();
        MarkedPrice = markedPrice is > 0 ? markedPrice : null;
        MarketKey = string.IsNullOrWhiteSpace(marketKey) ? null : marketKey.Trim();
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string? NameSi { get; private set; }
    public decimal Qty { get; private set; }
    public decimal Price { get; private set; }
    public string? Unit { get; private set; }
    public decimal? MarkedPrice { get; private set; }
    public string? MarketKey { get; private set; }

    /// <summary>Quantity times price, rounded.</summary>
    public decimal LineAmount => Money.Round(Qty * Price);

    /// <summary>True when the shop charged less than the printed marked price.</summary>
    public bool IsDiscounted => MarkedPrice > Price;

    /// <summary>Amount saved against the marked price for the whole line.</summary>
    public decimal Saved => IsDiscounted ? (MarkedPrice!.Value - Price) * Qty : 0;
}
