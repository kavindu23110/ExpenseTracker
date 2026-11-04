using ExpenseTracker.Domain.PriceWatch;

namespace ExpenseTracker.Application.Ports;

/// <summary>Reads the published price reference data.</summary>
public interface IPriceWatchRepository
{
    /// <summary>Loads market prices.</summary>
    Task<IReadOnlyList<MarketPrice>> GetMarketPricesAsync(CancellationToken ct);

    /// <summary>Loads inflation rates in display order.</summary>
    Task<IReadOnlyList<InflationRate>> GetInflationRatesAsync(CancellationToken ct);

    /// <summary>Loads price timelines in display order.</summary>
    Task<IReadOnlyList<PriceTimeline>> GetTimelinesAsync(CancellationToken ct);
}
