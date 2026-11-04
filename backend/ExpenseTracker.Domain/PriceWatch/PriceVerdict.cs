using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.PriceWatch;

/// <summary>The outcome of checking one item's price against a reference.</summary>
public sealed record PriceVerdict(string Text, string Tone, string Note)
{
    public const string Bad = "bad";
    public const string Good = "good";
    public const string Neutral = "neutral";

    /// <summary>Compares with the market price where one exists, else the marked price; null when neither applies.</summary>
    public static PriceVerdict? For(BillItem item, MarketPrice? market, decimal aboveMarketTolerancePercent)
    {
        if (market is not null) return AgainstMarket(item, market, aboveMarketTolerancePercent);
        return item.IsDiscounted ? AgainstMarked(item) : null;
    }

    private static PriceVerdict AgainstMarket(BillItem item, MarketPrice market, decimal tolerance)
    {
        var diff = (int)Math.Round((item.Price - market.Price) / market.Price * 100, MidpointRounding.AwayFromZero);
        var text = diff > 0 ? $"{diff}% above market" : diff < 0 ? $"{-diff}% below market" : "Same as market";
        var tone = diff > tolerance ? Bad : diff < 0 ? Good : Neutral;
        var packPriced = item.Unit is null && !item.Name.Contains("1kg", StringComparison.OrdinalIgnoreCase);
        return new(text, tone, market.Label + (packPriced ? " · assumes a 1 kg pack" : ""));
    }

    private static PriceVerdict AgainstMarked(BillItem item)
    {
        var marked = item.MarkedPrice!.Value;
        var diff = (int)Math.Round((marked - item.Price) / marked * 100, MidpointRounding.AwayFromZero);
        return new($"{diff}% under marked price", Good, $"Saved Rs {Money.Round(item.Saved):N2}");
    }
}
