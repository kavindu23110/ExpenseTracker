using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Application.Services;

/// <summary>Spending statistics from the household's bills.</summary>
public sealed class StatisticsService(IBillRepository bills, IItemMappingRepository mappings, ITodayProvider today)
{
    private const int TopCount = 5;

    /// <summary>Builds the statistics page.</summary>
    public async Task<StatisticsDto> GetAsync(CancellationToken ct)
    {
        var known = await mappings.GetAllAsync(ct);
        var months = SpendingStatistics.Months(await bills.GetAllAsync(ct), today.Today);
        return new(SpendingStatistics.AverageOfFinishedMonths(months), months.Select(m => new StatisticsMonthDto(
            m.Month.ToString("yyyy-MM"),
            m.Total,
            m.Bills.Count,
            Rows(m.Bills.GroupBy(b => b.Category).Select(g => new StatisticsRowDto(g.Key, null, g.Sum(b => b.Amount)))),
            Rows(m.Bills.GroupBy(b => b.Merchant).Select(g => new StatisticsRowDto(g.Key, null, g.Sum(b => b.Amount)))),
            Rows(ItemTotals.Summarise(m.Bills.SelectMany(b => b.Items), known).Select(i => new StatisticsRowDto(i.Name, i.NameSi, i.Amount))).Take(TopCount).ToList())).ToList());
    }

    private static IReadOnlyList<StatisticsRowDto> Rows(IEnumerable<StatisticsRowDto> rows) => rows.OrderByDescending(r => r.Amount).ToList();
}
