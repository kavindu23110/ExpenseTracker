namespace ExpenseTracker.Domain.Bills;

/// <summary>One purchase of a product.</summary>
public sealed record PricePoint(DateOnly Date, string Shop, decimal Price);

/// <summary>What a shop charged for a product on its latest purchase, and when.</summary>
public sealed record ShopPrice(string Shop, decimal Price, DateOnly Date);

/// <summary>What a product has cost across the household's own bills.</summary>
public sealed record ItemPriceHistory(string Name, string? NameSi, string? Unit, IReadOnlyList<PricePoint> Points, decimal ChangePercent, IReadOnlyList<ShopPrice> Shops);

/// <summary>Builds price histories for products bought more than once.</summary>
public static class PriceHistory
{
    /// <summary>Returns histories with the largest price rises first and each product's shops ranked by latest price, cheapest first; confirmed item mappings are combined.</summary>
    public static IReadOnlyList<ItemPriceHistory> Build(IEnumerable<Bill> bills, IReadOnlyCollection<ItemMapping>? mappings = null)
    {
        var product = ItemTotals.ProductKey(mappings);
        return bills.SelectMany(b => b.Items.Select(i => (Bill: b, Item: i)))
            .GroupBy(x => product(x.Item))
            .Where(g => g.Count() >= 2)
            .Select(g =>
            {
                var bought = g.OrderBy(x => x.Bill.Date).ThenBy(x => x.Bill.Id).ToList();
                var (first, last) = (bought[0].Item.Price, bought[^1].Item.Price);
                var shops = g.GroupBy(x => x.Bill.Merchant.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(s => s.OrderByDescending(x => x.Bill.Date).ThenByDescending(x => x.Bill.Id).First())
                    .Select(x => new ShopPrice(x.Bill.Merchant.Trim(), x.Item.Price, x.Bill.Date))
                    .OrderBy(s => s.Price).ThenBy(s => s.Shop, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                return new ItemPriceHistory(
                    bought[^1].Item.Name,
                    bought[^1].Item.NameSi,
                    bought[^1].Item.Unit,
                    bought.Select(x => new PricePoint(x.Bill.Date, x.Bill.Merchant, x.Item.Price)).ToList(),
                    first == 0 ? 0 : Math.Round((last - first) / first * 100, 1, MidpointRounding.AwayFromZero),
                    shops);
            })
            .OrderByDescending(h => h.ChangePercent)
            .ThenBy(h => h.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
