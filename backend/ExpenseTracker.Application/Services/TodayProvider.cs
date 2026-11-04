using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

/// <summary>Resolves today from the system clock and the configured time zone.</summary>
public sealed class TodayProvider(TimeProvider clock, IOptions<BillOptions> options) : ITodayProvider
{
    /// <inheritdoc />
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone)).DateTime);
}
