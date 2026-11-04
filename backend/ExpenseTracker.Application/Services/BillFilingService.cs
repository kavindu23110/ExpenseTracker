using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Bills;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

/// <inheritdoc />
public sealed class BillFilingService(IBillRepository bills, PantryService pantry, CategoryService categories, IOptions<BillOptions> options) : IBillFilingService
{
    /// <inheritdoc />
    public async Task<int> SaveAsync(SaveBillRequest r, UploadedFile? file, CancellationToken ct)
    {
        await categories.RequireAsync(r.Category, ct);
        var stored = file is null ? null : new BillFile(UploadGuard.Check(file, options.Value), file.Data);
        var content = new BillContent(
            r.Lang, r.Merchant, r.MerchantSi, r.Address, r.AddressSi, r.BillNo, ParseDate(r.Date), ParseTime(r.Time), r.Payment, r.PaymentSi, r.Paid,
            r.Items.Select(i => i.ToDomain()).ToList(), r.Charges.Select(c => c.ToDomain()).ToList());
        var bill = Bill.Create(content, r.Merchant, content.Date, r.Amount, r.Category, scanned: !r.Manual, file?.FileName ?? r.FileName, stored);
        if ((await bills.GetAllAsync(ct)).Any(bill.IsDuplicateOf)) throw new DomainException($"This bill is already saved: {bill.Merchant}, {bill.Date:yyyy-MM-dd}.");
        await bills.AddAsync(bill, r.Category != r.SuggestedCategory ? r.Category : null, ct);
        if (bill.Category == BillCategory.Groceries) await pantry.StockAsync(bill.Items, ct);
        return bill.Id;
    }

    /// <inheritdoc />
    public async Task<bool> ChangeCategoryAsync(int id, string? category, CancellationToken ct)
    {
        await categories.RequireAsync(category, ct);
        return await bills.ChangeCategoryAsync(id, category!, ct);
    }

    /// <inheritdoc />
    public async Task<BillDetailDto?> GetAsync(int id, CancellationToken ct)
    {
        var b = await bills.FindAsync(id, ct);
        if (b is null) return null;
        var file = b.HasFile ? await bills.FindFileAsync(id, ct) : null;
        return new BillDetailDto(
            b.Id, b.Lang, b.Merchant, b.MerchantSi, b.Address, b.BillNo, b.Category, b.Date.ToString("yyyy-MM-dd"), b.Time?.ToString("HH:mm"),
            b.Payment, b.Paid, b.Change, b.Subtotal, b.Amount, b.Saved, b.Scanned, b.FileName, file?.ContentType,
            b.Items.Select(BillItemDto.From).ToList(), b.Charges.Select(BillChargeDto.From).ToList());
    }

    /// <inheritdoc />
    public Task<BillFile?> GetFileAsync(int id, CancellationToken ct) => bills.FindFileAsync(id, ct);

    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", out var d) ? d : throw new DomainException("Enter the date as YYYY-MM-DD.");
    }

    private static TimeOnly? ParseTime(string? value) => TimeOnly.TryParse(value, out var t) ? t : null;
}
