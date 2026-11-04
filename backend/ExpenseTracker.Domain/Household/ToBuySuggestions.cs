using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Household;

/// <summary>Suggests what to buy from last month's grocery bills.</summary>
public static class ToBuySuggestions
{
    /// <summary>Last month's groceries not yet bought this month and not waiting on the list, largest spend first.</summary>
    public static IReadOnlyList<ItemTotal> Build(IReadOnlyCollection<Bill> bills, IReadOnlyCollection<string> openListKeys, IReadOnlyCollection<ItemMapping> mappings, DateOnly today)
    {
        var product = ItemTotals.ProductKey(mappings);
        var lastMonth = today.AddMonths(-1);
        var groceries = bills.Where(b => b.Category == BillCategory.Groceries).ToList();
        var boughtThisMonth = groceries.Where(b => InMonth(b.Date, today)).SelectMany(b => b.Items).Select(i => product(i).Item1).ToHashSet();
        var candidates = groceries.Where(b => InMonth(b.Date, lastMonth)).SelectMany(b => b.Items).Where(i => !boughtThisMonth.Contains(product(i).Item1));
        return ItemTotals.Summarise(candidates, mappings).Where(t => !t.Keys.Intersect(openListKeys).Any()).ToList();
    }

    /// <summary>What all of last month's grocery bills came to.</summary>
    public static decimal LastMonthTotal(IEnumerable<Bill> bills, DateOnly today) =>
        bills.Where(b => b.Category == BillCategory.Groceries && InMonth(b.Date, today.AddMonths(-1))).Sum(b => b.Amount);

    /// <summary>Every grocery item ever bought that is not waiting on the list, for suggesting while typing.</summary>
    public static IReadOnlyList<ItemTotal> Known(IReadOnlyCollection<Bill> bills, IReadOnlyCollection<string> openListKeys, IReadOnlyCollection<ItemMapping> mappings) =>
        ItemTotals.Summarise(bills.Where(b => b.Category == BillCategory.Groceries).SelectMany(b => b.Items), mappings)
            .Where(t => !t.Keys.Intersect(openListKeys).Any())
            .ToList();

    /// <summary>The line as written on the bill for an item's latest purchase last month, or else its latest purchase; null when never bought.</summary>
    public static BillItem? AmountToBuy(IReadOnlyCollection<Bill> bills, string name, DateOnly today)
    {
        var key = ItemTotals.KeyOf(name);
        var bought = bills.Where(b => b.Category == BillCategory.Groceries)
            .OrderBy(b => b.Date).ThenBy(b => b.Id)
            .SelectMany(b => b.Items.Select(i => (b.Date, Item: i)))
            .Where(x => ItemTotals.KeyOf(x.Item) == key)
            .ToList();
        var lastMonth = today.AddMonths(-1);
        var fromLastMonth = bought.Where(x => InMonth(x.Date, lastMonth)).ToList();
        return (fromLastMonth.Count > 0 ? fromLastMonth : bought).LastOrDefault().Item;
    }

    private static bool InMonth(DateOnly date, DateOnly month) => date.Year == month.Year && date.Month == month.Month;
}
