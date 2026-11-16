using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Household;

/// <summary>A shop that charges about the same every month and is not yet a recurring bill.</summary>
public sealed record SubscriptionCandidate(string Merchant, decimal Amount, int DayOfMonth, string Category, int Months);

/// <summary>Spots monthly charges in the bill history.</summary>
public static class SubscriptionFinder
{
    public const int MinMonths = 3;
    public const int LookbackMonths = 6;
    public const decimal Tolerance = 0.15m;

    /// <summary>Shops with a similar charge in at least three of the last six months.</summary>
    public static IReadOnlyList<SubscriptionCandidate> Find(IEnumerable<Bill> bills, IEnumerable<string> recurringNames, DateOnly today)
    {
        var since = new DateOnly(today.Year, today.Month, 1).AddMonths(-LookbackMonths);
        var known = recurringNames.Select(Normalise).ToHashSet();
        return bills.Where(b => b.Date >= since && b.Amount > 0)
            .GroupBy(b => Normalise(b.Merchant))
            .Where(g => !known.Contains(g.Key))
            .Select(g =>
            {
                var latest = g.OrderByDescending(b => b.Date).First();
                var typical = Median(g.Select(b => b.Amount));
                var similar = g.Where(b => Math.Abs(b.Amount - typical) <= typical * Tolerance).ToList();
                var months = similar.Select(b => (b.Date.Year, b.Date.Month)).Distinct().Count();
                // A subscription charges once a month; a shop visited more often than that is ordinary shopping.
                var monthly = months >= MinMonths && g.Count() == months;
                return monthly ? new SubscriptionCandidate(latest.Merchant, Money.Round(typical), (int)Math.Round(similar.Average(b => b.Date.Day)), latest.Category, months) : null;
            })
            .OfType<SubscriptionCandidate>()
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    private static string Normalise(string name) => string.Join(' ', name.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static decimal Median(IEnumerable<decimal> values)
    {
        var sorted = values.Order().ToList();
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2;
    }
}
