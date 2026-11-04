using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Services;

/// <summary>Builds the dashboard for a set of filters.</summary>
public interface IDashboardService
{
    /// <summary>Returns bills, summaries and price checks for the filters.</summary>
    Task<DashboardDto> GetAsync(DashboardQuery query, CancellationToken ct);
}
