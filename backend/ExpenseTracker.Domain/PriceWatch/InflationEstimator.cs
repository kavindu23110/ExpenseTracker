namespace ExpenseTracker.Domain.PriceWatch;

/// <summary>Estimates how much of today's spending is due to a year of price rises.</summary>
public static class InflationEstimator
{
    /// <summary>Spending minus what it would have cost a year ago at the given yearly rate.</summary>
    public static decimal ExtraCost(decimal spending, decimal ratePercent) => spending - spending / (1 + ratePercent / 100);
}
