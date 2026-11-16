using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Household;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

/// <inheritdoc />
public sealed class BillFilingService(IBillRepository bills, PantryService pantry, CategoryService categories, IStore<ItemCategoryRule> rules, IStore<AppUser> members, IOptions<BillOptions> options) : IBillFilingService
{
    /// <inheritdoc />
    public async Task<int> SaveAsync(SaveBillRequest r, UploadedFile? file, CancellationToken ct)
    {
        await categories.RequireAsync(r.Category, ct);
        await RequireItemCategoriesAsync(r.Items, ct);
        var stored = file is null ? null : new BillFile(UploadGuard.Check(file, options.Value), file.Data);
        var content = new BillContent(
            r.Lang, r.Merchant, r.MerchantSi, r.Address, r.AddressSi, r.BillNo, ParseDate(r.Date), ParseTime(r.Time), r.Payment, r.PaymentSi, r.Paid,
            r.Items.Select(i => i.ToDomain()).ToList(), r.Charges.Select(c => c.ToDomain()).ToList());
        var bill = Bill.Create(content, r.Merchant, content.Date, r.Amount, r.Category, scanned: !r.Manual, file?.FileName ?? r.FileName, stored);
        bill.SetPaidBy(await MemberNameAsync(r.PaidBy, ct));
        if ((await bills.GetAllAsync(ct)).Any(bill.IsDuplicateOf)) throw new DomainException($"This bill is already saved: {bill.Merchant}, {bill.Date:yyyy-MM-dd}.");
        await bills.AddAsync(bill, r.Category != r.SuggestedCategory ? r.Category : null, ct);
        await RememberAsync(r.Items, ct);
        await pantry.StockBillAsync(bill, ct);
        return bill.Id;
    }

    /// <inheritdoc />
    public async Task<bool> EditAsync(int id, EditBillRequest r, CancellationToken ct)
    {
        var date = ParseDate(r.Date);
        await RequireItemCategoriesAsync(r.Items, ct);
        var paidBy = await MemberNameAsync(r.PaidBy, ct);
        var preview = await bills.FindAsync(id, ct);
        if (preview is null) return false;
        preview.Edit(r.Merchant, date, r.Amount, r.Items.Select(i => i.ToDomain()).ToList());
        if ((await bills.GetAllAsync(ct)).Any(b => b.Id != id && b.IsDuplicateOf(preview))) throw new DomainException($"This would match a bill already saved: {preview.Merchant}, {preview.Date:yyyy-MM-dd}.");
        var updated = await bills.UpdateAsync(id, b => { b.Edit(r.Merchant, date, r.Amount, r.Items.Select(i => i.ToDomain()).ToList()); b.SetPaidBy(paidBy); }, ct);
        if (updated) await RememberAsync(r.Items, ct);
        return updated;
    }

    // A picked category is remembered for the item; setting it back to the bill's category forgets it.
    private async Task RememberAsync(IEnumerable<BillItemDto> items, CancellationToken ct)
    {
        var known = (await rules.ListAsync(ct)).ToDictionary(x => x.Key);
        foreach (var item in items.DistinctBy(i => ItemTotals.KeyOf(i.NameSi ?? i.Name)))
        {
            var key = ItemTotals.KeyOf(item.NameSi ?? item.Name);
            if (key.Length == 0) continue;
            var category = string.IsNullOrWhiteSpace(item.Category) ? null : item.Category.Trim();
            if (known.TryGetValue(key, out var rule))
            {
                if (category is null) { if (item.Remembered) rules.Remove(rule); }
                else rule.ChangeTo(category);
            }
            else if (category is not null) rules.Add(new ItemCategoryRule(key, category));
        }
        await rules.SaveAsync(ct);
    }

    private async Task<string?> MemberNameAsync(string? name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var member = (await members.ListAsync(ct)).FirstOrDefault(m => string.Equals(m.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        return member?.Name ?? throw new DomainException($"{name} is not a household member.");
    }

    /// <inheritdoc />
    public async Task<string> ExportCsvAsync(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var csv = new StringBuilder("Date,Shop,Bill category,Item,Item category,Qty,Unit,Price,Amount,Paid by,Bill total\r\n");
        var all = (await bills.GetAllAsync(ct)).Where(b => (from is null || b.Date >= from) && (to is null || b.Date <= to)).OrderBy(b => b.Date).ThenBy(b => b.Id);
        foreach (var b in all)
        {
            var date = b.Date.ToString("yyyy-MM-dd");
            foreach (var i in b.Items)
                Row(csv, date, b.Merchant, b.Category, i.NameSi is null ? i.Name : $"{i.Name} ({i.NameSi})", i.Category ?? b.Category, Num(i.Qty), i.Unit ?? "", Num(i.Price), Num(i.LineAmount), b.PaidBy ?? "", Num(b.Amount));
            foreach (var c in b.Charges)
                Row(csv, date, b.Merchant, b.Category, c.Label, b.Category, "", "", "", Num(c.Amount), b.PaidBy ?? "", Num(b.Amount));
        }
        return csv.ToString();
    }

    private static string Num(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static void Row(StringBuilder csv, params string[] cells) =>
        csv.Append(string.Join(',', cells.Select(Cell))).Append("\r\n");

    // Quotes cells that need it, and defuses values a spreadsheet would run as a formula.
    private static string Cell(string value)
    {
        if (value.Length > 0 && "=+-@\t\r".Contains(value[0]) && !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _)) value = "'" + value;
        return value.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }

    private async Task RequireItemCategoriesAsync(IEnumerable<BillItemDto> items, CancellationToken ct)
    {
        foreach (var category in items.Select(i => i.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct())
            await categories.RequireAsync(category!.Trim(), ct);
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
            b.Items.Select(BillItemDto.From).ToList(), b.Charges.Select(BillChargeDto.From).ToList(), b.PaidBy);
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
