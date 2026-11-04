using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.Infrastructure.Seed;

/// <summary>Applies migrations and seeds reference data, behind its setting.</summary>
public sealed class DatabaseInitialiser(AppDbContext db, IConfiguration config)
{
    /// <summary>Runs on startup.</summary>
    public async Task RunAsync(CancellationToken ct = default)
    {
        if (!config.GetValue<bool>("Database:ApplyMigrationsOnStartup")) return;
        await db.Database.MigrateAsync(ct);

        if (!await db.MarketPrices.AnyAsync(ct)) db.MarketPrices.AddRange(PriceWatchSeed.MarketPrices());
        if (!await db.InflationRates.AnyAsync(ct)) db.InflationRates.AddRange(PriceWatchSeed.Rates());

        await db.SaveChangesAsync(ct);
    }
}
