using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Categorisation;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

/// <inheritdoc />
public sealed class BillScanService(
    IBillReader reader,
    IBillRepository bills,
    IPriceWatchRepository priceWatch,
    CategoryService categories,
    IStore<ItemCategoryRule> rules,
    IOptions<BillOptions> options) : IBillScanService
{
    /// <inheritdoc />
    public async Task<BillDraftDto> ScanAsync(UploadedFile file, string? langHint, CancellationToken ct)
    {
        var contentType = UploadGuard.Check(file, options.Value);
        var hint = BillLanguage.IsValid(langHint) ? langHint : null;
        var reading = await reader.ReadAsync(file.Data, contentType, hint, await priceWatch.GetMarketPricesAsync(ct), await categories.AllAsync(ct), ct);
        return await ToDraftAsync(reading, file.FileName, ct);
    }

    // A remembered pick beats the reader's guess.
    private async Task<List<BillItemDto>> WithRulesAsync(IEnumerable<BillItem> items, CancellationToken ct)
    {
        var known = (await rules.ListAsync(ct)).ToDictionary(r => r.Key, r => r.Category);
        var valid = await categories.AllAsync(ct);
        return items.Select(i => BillItemDto.From(i) is var dto && known.TryGetValue(ItemTotals.KeyOf(i), out var c) && valid.Contains(c)
            ? dto with { Category = c, Remembered = true }
            : dto).ToList();
    }

    private async Task<BillDraftDto> ToDraftAsync(BillReading reading, string fileName, CancellationToken ct)
    {
        var c = reading.Content;
        var guess = BillCategoriser.Suggest(c, await bills.FindLearnedCategoryAsync(c.Merchant, ct));
        var amount = reading.PrintedTotal ?? c.Total;
        return new BillDraftDto(
            c.Lang, c.Merchant, c.MerchantSi, c.Address, c.AddressSi, c.BillNo, c.Date?.ToString("yyyy-MM-dd"), c.Time?.ToString("HH:mm"),
            c.Payment, c.PaymentSi, c.Paid, amount, c.Total, Math.Abs(amount - c.Total) > options.Value.TotalMismatchTolerance, c.Saved, fileName,
            new SuggestionDto(guess.Category, guess.Confidence, guess.Reason),
            await WithRulesAsync(c.Items, ct), c.Charges.Select(BillChargeDto.From).ToList());
    }
}
