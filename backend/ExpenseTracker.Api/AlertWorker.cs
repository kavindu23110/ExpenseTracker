using ExpenseTracker.Application;
using ExpenseTracker.Application.Services;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Api;

/// <summary>Checks every half hour whether today's WhatsApp reminders are due.</summary>
public sealed class AlertWorker(IServiceScopeFactory scopes, TimeProvider clock, IOptions<BillOptions> options, ILogger<AlertWorker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(30), clock);
        do
        {
            try
            {
                var hour = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone)).Hour;
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<AlertService>().SendDailyAsync(hour, stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "The daily reminders could not be sent.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
