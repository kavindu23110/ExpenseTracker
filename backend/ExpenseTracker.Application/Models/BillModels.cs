using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Application.Models;

/// <summary>A line item as sent over the API.</summary>
public sealed record BillItemDto(string Name, string? NameSi, decimal Qty, decimal Price, string? Unit, decimal? MarkedPrice, string? MarketKey, decimal Amount)
{
    /// <summary>Maps from the domain.</summary>
    public static BillItemDto From(BillItem i) => new(i.Name, i.NameSi, i.Qty, i.Price, i.Unit, i.MarkedPrice, i.MarketKey, i.LineAmount);

    /// <summary>Maps to the domain, re-running its validation.</summary>
    public BillItem ToDomain() => new(Name, Qty, Price, Unit, NameSi, MarkedPrice, MarketKey);
}

/// <summary>A charge as sent over the API.</summary>
public sealed record BillChargeDto(string Label, string? LabelSi, decimal Amount)
{
    /// <summary>Maps from the domain.</summary>
    public static BillChargeDto From(BillCharge c) => new(c.Label, c.LabelSi, c.Amount);

    /// <summary>Maps to the domain.</summary>
    public BillCharge ToDomain() => new(Label, Amount, LabelSi);
}

/// <summary>One row in the recent bills table.</summary>
public sealed record BillRowDto(int Id, string Merchant, string Category, bool Scanned, string Lang, string Date, decimal Amount, int ItemCount);

/// <summary>A bill with everything the detail panel shows.</summary>
public sealed record BillDetailDto(
    int Id, string Lang, string Merchant, string? MerchantSi, string? Address, string? BillNo, string Category, string Date, string? Time,
    string? Payment, decimal? Paid, decimal? Change, decimal Subtotal, decimal Amount, decimal Saved, bool Scanned, string? FileName, string? FileContentType,
    IReadOnlyList<BillItemDto> Items, IReadOnlyList<BillChargeDto> Charges);

/// <summary>A category suggestion as sent over the API.</summary>
public sealed record ChangeBillCategoryRequest(string? Category);

/// <summary>A suggested category.</summary>
public sealed record SuggestionDto(string Category, int Confidence, string Reason);

/// <summary>A scanned bill waiting for the user to check it.</summary>
public sealed record BillDraftDto(
    string Lang, string Merchant, string? MerchantSi, string? Address, string? AddressSi, string? BillNo, string? Date, string? Time,
    string? Payment, string? PaymentSi, decimal? Paid, decimal Amount, decimal ComputedTotal, bool TotalMismatch, decimal Saved, string FileName,
    SuggestionDto Suggestion, IReadOnlyList<BillItemDto> Items, IReadOnlyList<BillChargeDto> Charges);

/// <summary>A checked draft to file, with the user's edits.</summary>
public sealed record SaveBillRequest(
    string Lang, string Merchant, string? MerchantSi, string? Address, string? AddressSi, string? BillNo, string? Date, string? Time,
    string? Payment, string? PaymentSi, decimal? Paid, decimal Amount, string Category, string SuggestedCategory, string? FileName,
    IReadOnlyList<BillItemDto> Items, IReadOnlyList<BillChargeDto> Charges, bool Manual = false);

/// <summary>A file uploaded with a request.</summary>
public sealed record UploadedFile(string FileName, byte[] Data);
