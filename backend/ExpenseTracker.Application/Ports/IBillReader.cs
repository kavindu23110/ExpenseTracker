using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.PriceWatch;

namespace ExpenseTracker.Application.Ports;

/// <summary>Reads the contents of a bill photo or PDF.</summary>
public interface IBillReader
{
    /// <summary>Extracts a bill; langHint is en, si or null to detect. Each item may get a suggested category from the list.</summary>
    Task<BillReading> ReadAsync(byte[] data, string contentType, string? langHint, IReadOnlyList<MarketPrice> marketPrices, IReadOnlyList<string> categories, CancellationToken ct);
}

/// <summary>A reader's result: the content plus the total printed on the bill, if legible.</summary>
public sealed record BillReading(BillContent Content, decimal? PrintedTotal);
