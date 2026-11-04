namespace ExpenseTracker.Domain.Bills;

/// <summary>Specification for the dashboard filters, excluding the category chip.</summary>
public sealed record BillFilter(string? Query, BillPeriod Period, string? Shop, string? Lang, bool? Scanned)
{
    /// <summary>True when the bill passes every active filter.</summary>
    public bool Matches(Bill b)
    {
        if (!Period.Contains(b.Date)) return false;
        if (!string.IsNullOrEmpty(Shop) && b.Merchant != Shop) return false;
        if (!string.IsNullOrEmpty(Lang) && b.Lang != Lang) return false;
        if (Scanned is not null && b.Scanned != Scanned) return false;
        return string.IsNullOrWhiteSpace(Query) || MatchesQuery(b, Query.Trim());
    }

    private static bool MatchesQuery(Bill b, string q)
    {
        var parts = new[] { b.Merchant, b.MerchantSi, b.BillNo, b.Category }.Concat(b.Items.SelectMany(i => new[] { i.Name, i.NameSi }));
        return parts.Any(p => p is not null && p.Contains(q, StringComparison.OrdinalIgnoreCase));
    }
}
