using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.PriceWatch;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

/// <inheritdoc />
public sealed class DashboardService(
    IBillRepository bills,
    IPriceWatchRepository priceWatch,
    IItemMappingRepository itemMappings,
    IBillReaderStatus readerStatus,
    CategoryService categories,
    ITodayProvider today,
    IOptions<BillOptions> options) : IDashboardService
{
    /// <inheritdoc />
    public async Task<DashboardDto> GetAsync(DashboardQuery q, CancellationToken ct)
    {
        var all = await bills.GetAllAsync(ct);
        var rates = await priceWatch.GetInflationRatesAsync(ct);
        var markets = (await priceWatch.GetMarketPricesAsync(ct)).ToDictionary(m => m.Key);
        var timelines = await priceWatch.GetTimelinesAsync(ct);
        var mappings = await itemMappings.GetAllAsync(ct);
        var allCategories = await categories.AllAsync(ct);

        var periods = BillPeriod.ForToday(today.Today);
        var period = periods.FirstOrDefault(p => p.Key == q.Period) ?? periods[0];
        bool? scanned = q.Source switch { "scanned" => true, "manual" => false, _ => null };
        var filter = new BillFilter(q.Q, period, q.Shop, q.Lang, scanned);

        var baseSet = all.Where(filter.Matches).ToList();
        var shown = baseSet.Where(b => string.IsNullOrEmpty(q.Category) || b.Category == q.Category).ToList();
        var byCategory = shown.GroupBy(b => b.Category).Select(g => (Name: g.Key, Amount: g.Sum(b => b.Amount))).OrderByDescending(c => c.Amount).ToList();
        var total = shown.Sum(b => b.Amount);
        var max = byCategory.Count > 0 ? byCategory[0].Amount : 1;

        var headline = rates.FirstOrDefault(r => r.Category is null)?.Rate ?? 0;
        var inflationRows = byCategory.Select(c =>
        {
            var rate = rates.FirstOrDefault(r => r.Category == c.Name)?.Rate ?? headline;
            return new InflationRowDto(c.Name, rate, InflationEstimator.ExtraCost(c.Amount, rate));
        }).ToList();

        var discounts = shown.Where(b => b.Saved > 0)
            .Select(b => new DiscountRowDto(b.Id, b.Merchant, b.Saved, Math.Round(b.Saved / (b.Amount + b.Saved) * 100, 1, MidpointRounding.AwayFromZero)))
            .ToList();

        return new DashboardDto(
            today.Today.ToString("yyyy-MM-dd"),
            period.Label,
            all.Count,
            periods.Select(p => new OptionDto(p.Key, p.Label)).ToList(),
            all.Select(b => b.Merchant).Distinct().Order(StringComparer.OrdinalIgnoreCase).ToList(),
            allCategories.Where(c => baseSet.Any(b => b.Category == c)).ToList(),
            allCategories,
            shown.Select(b => new BillRowDto(b.Id, b.Merchant, b.Category, b.Scanned, b.Lang, b.Date.ToString("yyyy-MM-dd"), b.Amount, b.Items.Count)).ToList(),
            new SummaryDto(total, shown.Count, shown.Count(b => b.Lang == BillLanguage.Sinhala), byCategory.Count > 0 ? byCategory[0].Name : null),
            byCategory.Select(c => new BreakdownDto(c.Name, c.Amount, Percent(c.Amount, total), Math.Max(4, Percent(c.Amount, max)))).ToList(),
            rates.Where(r => r.PreviousRate is not null).Select(r => new InflationCardDto(r.Label, r.Rate, r.PreviousRate, r.Note)).ToList(),
            headline,
            inflationRows,
            inflationRows.Sum(r => r.Extra),
            discounts,
            discounts.Sum(d => d.Saved),
            BuildChecks(shown, markets),
            BuildItemMonths(all, mappings),
            ItemTotals.FindQuestions(all.SelectMany(b => b.Items), mappings).Select(q => new ItemQuestionDto(q.KeyA, q.NameA, q.KeyB, q.NameB)).ToList(),
            timelines.Select(ToDto).ToList(),
            readerStatus.IsAvailable);
    }

    private static List<ItemMonthDto> BuildItemMonths(IEnumerable<Bill> all, IReadOnlyCollection<ItemMapping> mappings) =>
        all.GroupBy(b => b.Date.ToString("yyyy-MM"))
            .OrderByDescending(g => g.Key)
            .Select(g =>
            {
                var items = ItemTotals.Summarise(g.SelectMany(b => b.Items), mappings);
                return new ItemMonthDto(g.Key, items.Sum(i => i.Amount), items.Select(i => new ItemTotalDto(i.Name, i.NameSi, i.Qty, i.Unit, i.Amount, i.Keys, i.Names)).ToList());
            })
            .ToList();

    private List<PriceCheckDto> BuildChecks(IEnumerable<Bill> shown, Dictionary<string, MarketPrice> markets)
    {
        var tolerance = options.Value.AboveMarketTolerancePercent;
        var checks = new List<PriceCheckDto>();
        foreach (var bill in shown)
        {
            foreach (var item in bill.Items)
            {
                var market = item.MarketKey is null ? null : markets.GetValueOrDefault(item.MarketKey);
                var verdict = PriceVerdict.For(item, market, tolerance);
                if (verdict is null) continue;
                checks.Add(new PriceCheckDto(item.Name, item.NameSi, bill.Merchant, item.Price, item.IsDiscounted ? item.MarkedPrice : null, market?.Price, item.Unit, market?.Unit, verdict.Text, verdict.Tone, verdict.Note));
            }
        }

        return checks.OrderBy(c => c.Tone switch { PriceVerdict.Bad => 0, PriceVerdict.Neutral => 1, _ => 2 }).ToList();
    }

    private static TimelineDto ToDto(PriceTimeline t) =>
        new(t.Title, t.Summary, t.Foot, t.Points.OrderBy(p => p.Sort).Select(p => new TimelinePointDto(p.Label, p.Price, t.BarPercent(p), p.IsUser)).ToList());

    private static int Percent(decimal part, decimal whole) => whole == 0 ? 0 : (int)Math.Round(part / whole * 100, MidpointRounding.AwayFromZero);
}
