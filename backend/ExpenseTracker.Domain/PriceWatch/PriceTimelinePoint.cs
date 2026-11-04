namespace ExpenseTracker.Domain.PriceWatch;

/// <summary>One labelled price on a timeline; IsUser marks what the user paid.</summary>
public sealed class PriceTimelinePoint
{
    private PriceTimelinePoint() { Label = string.Empty; }

    /// <summary>Creates a point.</summary>
    public PriceTimelinePoint(int sort, string label, decimal price, bool isUser = false)
    {
        Sort = sort;
        Label = label;
        Price = price;
        IsUser = isUser;
    }

    public int Id { get; private set; }
    public int Sort { get; private set; }
    public string Label { get; private set; }
    public decimal Price { get; private set; }
    public bool IsUser { get; private set; }
}
