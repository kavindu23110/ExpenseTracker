namespace ExpenseTracker.Application.Models;

/// <summary>The filters a dashboard request can carry.</summary>
public sealed record DashboardQuery(string? Q, string? Period, string? Shop, string? Lang, string? Source, string? Category);

/// <summary>An option in a filter list.</summary>
public sealed record OptionDto(string Value, string Label);

/// <summary>The headline numbers.</summary>
public sealed record SummaryDto(decimal Total, int Count, int SinhalaCount, string? TopCategory);

/// <summary>One bar in the category breakdown.</summary>
public sealed record BreakdownDto(string Name, decimal Amount, int Share, int Percent);

/// <summary>A published inflation card.</summary>
public sealed record InflationCardDto(string Label, decimal Rate, decimal? PreviousRate, string? Note);

/// <summary>Inflation's estimated effect on one category.</summary>
public sealed record InflationRowDto(string Name, decimal Rate, decimal Extra);

/// <summary>A bill's savings against marked prices.</summary>
public sealed record DiscountRowDto(int BillId, string Shop, decimal Saved, decimal Percent);

/// <summary>One row in the item price checks.</summary>
public sealed record PriceCheckDto(string Name, string? NameSi, string Shop, decimal Paid, decimal? Marked, decimal? Market, string? Unit, string? MarketUnit, string Verdict, string Tone, string Note);

/// <summary>One product's total in a month.</summary>
public sealed record ItemTotalDto(string Name, string? NameSi, decimal Qty, string? Unit, decimal Amount, IReadOnlyList<string> Keys, IReadOnlyList<string> Names);

/// <summary>Two item names the user is asked to confirm or keep apart.</summary>
public sealed record ItemQuestionDto(string KeyA, string NameA, string KeyB, string NameB);

/// <summary>The user's answer on whether items are the same product.</summary>
public sealed record ItemMappingRequest(IReadOnlyList<string>? Keys, bool Same);

/// <summary>A month's spending by item; Month is yyyy-MM.</summary>
public sealed record ItemMonthDto(string Month, decimal Total, IReadOnlyList<ItemTotalDto> Items);

/// <summary>One bar on a price timeline.</summary>
public sealed record TimelinePointDto(string Label, decimal Price, int Percent, bool IsUser);

/// <summary>A product's price history.</summary>
public sealed record TimelineDto(string Title, string Summary, string Foot, IReadOnlyList<TimelinePointDto> Points);

/// <summary>Everything the dashboard shows for a set of filters.</summary>
public sealed record DashboardDto(
    string Today,
    string PeriodLabel,
    int AllCount,
    IReadOnlyList<OptionDto> Periods,
    IReadOnlyList<string> Shops,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> AllCategories,
    IReadOnlyList<BillRowDto> Bills,
    SummaryDto Summary,
    IReadOnlyList<BreakdownDto> Breakdown,
    IReadOnlyList<InflationCardDto> InflationCards,
    decimal HeadlineRate,
    IReadOnlyList<InflationRowDto> InflationRows,
    decimal InflationTotal,
    IReadOnlyList<DiscountRowDto> Discounts,
    decimal DiscountTotal,
    IReadOnlyList<PriceCheckDto> Checks,
    IReadOnlyList<ItemMonthDto> ItemMonths,
    IReadOnlyList<ItemQuestionDto> ItemQuestions,
    IReadOnlyList<TimelineDto> Timelines,
    bool ReaderAvailable);
