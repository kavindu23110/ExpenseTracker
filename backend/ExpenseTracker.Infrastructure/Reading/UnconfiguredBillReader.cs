using ExpenseTracker.Application;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.PriceWatch;

namespace ExpenseTracker.Infrastructure.Reading;

/// <summary>Null object used when no bill reader is set up; never invents bill data.</summary>
public sealed class UnconfiguredBillReader : IBillReader
{
    /// <inheritdoc />
    public Task<BillReading> ReadAsync(byte[] data, string contentType, string? langHint, IReadOnlyList<MarketPrice> marketPrices, CancellationToken ct) =>
        throw new BillReaderUnavailableException("Bill reading is not set up yet. Install Claude Code or add a Claude API key to the backend to read real bills.");
}
