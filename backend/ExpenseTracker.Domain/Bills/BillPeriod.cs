using System.Globalization;

namespace ExpenseTracker.Domain.Bills;

/// <summary>A named date window relative to today.</summary>
public sealed record BillPeriod(string Key, string Label, DateOnly? From, DateOnly? To)
{
    public const string AllKey = "all";

    /// <summary>The periods offered to the user for a given day.</summary>
    public static IReadOnlyList<BillPeriod> ForToday(DateOnly today)
    {
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var lastMonth = monthStart.AddMonths(-1);
        return
        [
            new(AllKey, "All time", null, null),
            new("month", monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture), monthStart, null),
            new("week", "Last 7 days", today.AddDays(-6), null),
            new("lastMonth", lastMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture), lastMonth, monthStart.AddDays(-1)),
        ];
    }

    /// <summary>True when the date falls inside the period.</summary>
    public bool Contains(DateOnly date) => (From is null || date >= From) && (To is null || date <= To);
}
