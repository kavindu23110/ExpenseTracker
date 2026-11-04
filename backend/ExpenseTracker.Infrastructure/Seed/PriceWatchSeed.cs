using ExpenseTracker.Domain.PriceWatch;

namespace ExpenseTracker.Infrastructure.Seed;

/// <summary>The published figures from the design, as of October 2026.</summary>
internal static class PriceWatchSeed
{
    public static MarketPrice[] MarketPrices() =>
    [
        new("potato-imported", 189, "kg", "Pettah, imported potato, 19 Oct 2026", "imported potatoes, sold loose"),
        new("dhal-red-1kg", 280, "kg", "Pettah, red dhal, 19 Oct 2026", "red dhal / mysore dhal"),
        new("sugar-1kg", 240, "kg", "Pettah, white sugar, 19 Oct 2026", "white sugar"),
    ];

    public static InflationRate[] Rates() =>
    [
        new(0, "All prices (Colombo)", null, 8.3m, 8.0m, "Oct 2026"),
        new(1, "Food", "Groceries", 8.8m, 8.7m),
        new(2, "Restaurants & hotels", "Dining", 13.4m, 11.5m),
        new(3, "Transport", "Transport", 17.6m, 16.3m),
        new(4, "Health", "Health", 2.6m, null),
    ];
}
