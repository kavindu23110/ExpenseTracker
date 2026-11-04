using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.PriceWatch;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence;

/// <inheritdoc />
public sealed class PriceWatchRepository(AppDbContext db) : IPriceWatchRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<MarketPrice>> GetMarketPricesAsync(CancellationToken ct) =>
        await db.MarketPrices.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<InflationRate>> GetInflationRatesAsync(CancellationToken ct) =>
        await db.InflationRates.AsNoTracking().OrderBy(r => r.Sort).ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PriceTimeline>> GetTimelinesAsync(CancellationToken ct) =>
        await db.PriceTimelines.AsNoTracking().OrderBy(t => t.Sort).ToListAsync(ct);
}
