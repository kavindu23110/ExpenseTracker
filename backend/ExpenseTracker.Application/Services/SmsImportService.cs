using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Categorisation;
using ExpenseTracker.Domain.Household;

namespace ExpenseTracker.Application.Services;

/// <summary>Turns pasted bank messages into expenses, after the user checks them.</summary>
public sealed class SmsImportService(IBillRepository bills, IStore<Bill> billStore, CategoryService categories, ITodayProvider today)
{
    private const string UnknownShop = "Bank payment";
    private const string LineName = "Bank SMS payment";

    /// <summary>Finds payments and flags any already imported. Message text is not stored.</summary>
    public async Task<IReadOnlyList<SmsDraftDto>> PreviewAsync(string? text, CancellationToken ct)
    {
        var imported = await ImportedReferencesAsync(ct);
        var drafts = new List<SmsDraftDto>();
        foreach (var p in BankSmsParser.Parse(text).DistinctBy(p => p.Reference))
        {
            var merchant = p.Merchant ?? UnknownShop;
            var date = p.Date ?? today.Today;
            var guess = BillCategoriser.Suggest(Content(merchant, date, p.Amount, p.Reference), await bills.FindLearnedCategoryAsync(merchant, ct));
            drafts.Add(new SmsDraftDto(p.Amount, merchant, date.ToString("yyyy-MM-dd"), guess.Category, p.Reference, imported.Contains(p.Reference)));
        }

        return drafts;
    }

    /// <summary>Saves checked payments, skipping any already imported; returns how many were added.</summary>
    public async Task<int> ImportAsync(IReadOnlyList<SmsImportItem>? items, CancellationToken ct)
    {
        var imported = await ImportedReferencesAsync(ct);
        var added = 0;
        foreach (var item in items ?? [])
        {
            if (string.IsNullOrWhiteSpace(item.Reference) || !imported.Add(item.Reference)) continue;
            if (!DateOnly.TryParseExact(item.Date, "yyyy-MM-dd", out var date)) throw new DomainException("Enter dates as YYYY-MM-DD.");
            await categories.RequireAsync(item.Category, ct);
            var content = Content(item.Merchant, date, item.Amount, item.Reference);
            billStore.Add(Bill.Create(content, item.Merchant, date, item.Amount, item.Category, scanned: false, null));
            added++;
        }

        await billStore.SaveAsync(ct);
        return added;
    }

    private async Task<HashSet<string>> ImportedReferencesAsync(CancellationToken ct) =>
        (await bills.GetAllAsync(ct)).Where(b => b.BillNo is not null).Select(b => b.BillNo!).ToHashSet();

    private static BillContent Content(string merchant, DateOnly date, decimal amount, string reference) =>
        new(BillLanguage.English, merchant, null, null, null, reference, date, null, "Bank SMS", null, null, [new BillItem(LineName, 1, amount)], []);
}
