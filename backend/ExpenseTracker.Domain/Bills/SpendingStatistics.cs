namespace ExpenseTracker.Domain.Bills;

/// <summary>One calendar month's spending.</summary>
public sealed record MonthSpending(DateOnly Month, decimal Total, IReadOnlyList<Bill> Bills);

/// <summary>Works out spending month by month.</summary>
public static class SpendingStatistics
{
    private const int MaxMonths = 12;
    private const int MinMonths = 6;

    /// <summary>Up to a year of months, oldest first, starting no earlier than needed but always showing six.</summary>
    public static IReadOnlyList<MonthSpending> Months(IReadOnlyCollection<Bill> bills, DateOnly today)
    {
        var current = new DateOnly(today.Year, today.Month, 1);
        var byMonth = bills.ToLookup(b => new DateOnly(b.Date.Year, b.Date.Month, 1));
        var months = Enumerable.Range(0, MaxMonths).Select(i => current.AddMonths(i - MaxMonths + 1))
            .Select(m => new MonthSpending(m, byMonth[m].Sum(b => b.Amount), byMonth[m].ToList())).ToList();
        var leadingEmpty = months.TakeWhile(m => m.Bills.Count == 0).Count();
        return months.Skip(Math.Min(leadingEmpty, MaxMonths - MinMonths)).ToList();
    }

    /// <summary>The average of finished months since the first one with a bill; null until one has finished.</summary>
    public static decimal? AverageOfFinishedMonths(IReadOnlyList<MonthSpending> months)
    {
        var finished = months.Take(months.Count - 1).SkipWhile(m => m.Bills.Count == 0).ToList();
        return finished.Count == 0 ? null : Money.Round(finished.Average(m => m.Total));
    }
}
