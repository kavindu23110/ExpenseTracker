using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Household;

namespace ExpenseTracker.Application.Services;

/// <summary>The pantry and shopping list.</summary>
public sealed class PantryService(IStore<PantryItem> pantry, IStore<ShoppingItem> shopping, IBillRepository bills, IItemMappingRepository mappings, IStore<ToBuyMonthlyFill> fills, ITodayProvider today)
{
    /// <summary>Loads both lists, first filling the shopping list if this month has not been filled.</summary>
    public async Task<PantryDto> GetAsync(CancellationToken ct)
    {
        await FillForMonthAsync(ct);
        return new(
        (await pantry.ListAsync(ct)).OrderBy(p => p.IsOut).ThenBy(p => p.Name).Select(p => new PantryItemDto(p.Id, p.Name, p.Qty, p.Unit, p.IsOut)).ToList(),
        (await shopping.ListAsync(ct)).OrderBy(s => s.Done).ThenBy(s => s.Name).Select(s => new ShoppingItemDto(s.Id, s.Name, s.Done, s.Qty, s.Unit)).ToList());
    }

    /// <summary>Suggests items for the list from last month's grocery bills.</summary>
    public async Task<IReadOnlyList<ToBuySuggestionDto>> GetSuggestionsAsync(CancellationToken ct)
    {
        var open = (await shopping.ListAsync(ct)).Where(s => !s.Done).Select(s => s.Key).ToList();
        var all = await bills.GetAllAsync(ct);
        return ToBuySuggestions.Build(all, open, await mappings.GetAllAsync(ct), today.Today)
            .Select(t =>
            {
                var line = ToBuySuggestions.AmountToBuy(all, t.NameSi ?? t.Name, today.Today);
                return new ToBuySuggestionDto(t.Name, t.NameSi, line?.Qty ?? t.Qty, line is null ? t.Unit : line.Unit);
            }).ToList();
    }

    /// <summary>Totals last month's grocery bills.</summary>
    public async Task<ToBuyLastMonthDto> GetLastMonthAsync(CancellationToken ct)
    {
        var month = today.Today.AddMonths(-1);
        return new(month.ToString("yyyy-MM"), ToBuySuggestions.LastMonthTotal(await bills.GetAllAsync(ct), today.Today));
    }

    /// <summary>Lists grocery items bought before, for suggesting while typing.</summary>
    public async Task<IReadOnlyList<ToBuySuggestionDto>> GetKnownItemsAsync(CancellationToken ct)
    {
        var open = (await shopping.ListAsync(ct)).Where(s => !s.Done).Select(s => s.Key).ToList();
        return ToBuySuggestions.Known(await bills.GetAllAsync(ct), open, await mappings.GetAllAsync(ct))
            .Select(t => new ToBuySuggestionDto(t.Name, t.NameSi, t.Qty, t.Unit)).ToList();
    }

    /// <summary>Adds bought items to the pantry and ticks them off the shopping list.</summary>
    public async Task StockAsync(IEnumerable<BillItem> items, CancellationToken ct)
    {
        var stock = await pantry.ListAsync(ct);
        var list = await shopping.ListAsync(ct);
        foreach (var item in items)
        {
            var key = ItemTotals.KeyOf(item);
            Stock(stock, key, item.NameSi ?? item.Name, item.Qty, item.Unit);
            foreach (var line in list.Where(s => s.Key == key)) line.SetDone(true);
        }

        await pantry.SaveAsync(ct);
    }

    /// <summary>Adds an item by hand.</summary>
    public async Task AddAsync(AddPantryRequest r, CancellationToken ct)
    {
        Stock(await pantry.ListAsync(ct), ItemTotals.KeyOf(r.Name ?? ""), r.Name ?? "", r.Qty, string.IsNullOrWhiteSpace(r.Unit) ? null : r.Unit.Trim());
        await pantry.SaveAsync(ct);
    }

    /// <summary>Changes a quantity; running out puts the item on the shopping list. False when missing.</summary>
    public async Task<bool> AdjustAsync(int id, decimal change, CancellationToken ct)
    {
        if (await pantry.FindAsync(id, ct) is not { } item) return false;
        item.Adjust(change);
        if (item.IsOut) await AddToListAsync(item.Key, item.Name, ct);
        await pantry.SaveAsync(ct);
        return true;
    }

    /// <summary>Marks an item finished and puts it on the shopping list. False when missing.</summary>
    public async Task<bool> UseUpAsync(int id, CancellationToken ct)
    {
        if (await pantry.FindAsync(id, ct) is not { } item) return false;
        item.UseUp();
        await AddToListAsync(item.Key, item.Name, ct);
        await pantry.SaveAsync(ct);
        return true;
    }

    /// <summary>Deletes a pantry item. False when missing.</summary>
    public async Task<bool> RemoveAsync(int id, CancellationToken ct)
    {
        if (await pantry.FindAsync(id, ct) is not { } item) return false;
        pantry.Remove(item);
        await pantry.SaveAsync(ct);
        return true;
    }

    /// <summary>Adds a shopping list line.</summary>
    public async Task AddShoppingAsync(string? name, CancellationToken ct)
    {
        await AddToListAsync(ItemTotals.KeyOf(name ?? ""), name ?? "", ct);
        await shopping.SaveAsync(ct);
    }

    /// <summary>Ticks or unticks a line. False when missing.</summary>
    public async Task<bool> SetDoneAsync(int id, bool done, CancellationToken ct)
    {
        if (await shopping.FindAsync(id, ct) is not { } line) return false;
        line.SetDone(done);
        await shopping.SaveAsync(ct);
        return true;
    }

    /// <summary>Deletes a line. False when missing.</summary>
    public async Task<bool> SetShoppingAmountAsync(int id, decimal? qty, string? unit, CancellationToken ct)
    {
        if (await shopping.FindAsync(id, ct) is not { } line) return false;
        line.ChangeAmount(qty, unit);
        await shopping.SaveAsync(ct);
        return true;
    }

    /// <summary>Removes a shopping list line. False when missing.</summary>
    public async Task<bool> RemoveShoppingAsync(int id, CancellationToken ct)
    {
        if (await shopping.FindAsync(id, ct) is not { } line) return false;
        shopping.Remove(line);
        await shopping.SaveAsync(ct);
        return true;
    }

    private void Stock(List<PantryItem> stock, string key, string name, decimal qty, string? unit)
    {
        var existing = stock.FirstOrDefault(p => p.Key == key && string.Equals(p.Unit, unit, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.Adjust(qty);
            return;
        }

        var added = new PantryItem(key, name, qty, unit);
        pantry.Add(added);
        stock.Add(added);
    }

    private async Task FillForMonthAsync(CancellationToken ct)
    {
        var month = today.Today.ToString("yyyy-MM");
        if (await fills.FindAsync(month, ct) is not null) return;
        var open = (await shopping.ListAsync(ct)).Where(s => !s.Done).Select(s => s.Key).ToList();
        var items = ToBuySuggestions.Build(await bills.GetAllAsync(ct), open, await mappings.GetAllAsync(ct), today.Today);
        foreach (var name in items.Select(t => t.NameSi ?? t.Name).DistinctBy(ItemTotals.KeyOf)) await AddToListAsync(ItemTotals.KeyOf(name), name, ct);
        fills.Add(new ToBuyMonthlyFill(month));
        await fills.SaveAsync(ct);
    }

    private async Task AddToListAsync(string key, string name, CancellationToken ct)
    {
        var existing = (await shopping.ListAsync(ct)).FirstOrDefault(s => s.Key == key);
        var line = existing ?? new ShoppingItem(key, name);
        // A line still waiting to be bought keeps its amount, which may have been entered by hand.
        if (existing is null || existing.Done)
        {
            var amount = ToBuySuggestions.AmountToBuy(await bills.GetAllAsync(ct), name, today.Today);
            line.SetAmount(amount?.Qty, amount?.Unit);
        }

        if (existing is null) shopping.Add(line);
        else existing.SetDone(false);
    }
}
