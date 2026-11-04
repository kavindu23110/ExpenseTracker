namespace ExpenseTracker.Domain.Bills;

/// <summary>What a bill says, before it is filed; fields may be missing after a scan.</summary>
public sealed record BillContent(
    string Lang,
    string Merchant,
    string? MerchantSi,
    string? Address,
    string? AddressSi,
    string? BillNo,
    DateOnly? Date,
    TimeOnly? Time,
    string? Payment,
    string? PaymentSi,
    decimal? Paid,
    IReadOnlyList<BillItem> Items,
    IReadOnlyList<BillCharge> Charges)
{
    /// <summary>True when the bill is written in Sinhala.</summary>
    public bool IsSinhala => Lang == BillLanguage.Sinhala;

    /// <summary>Sum of the line items.</summary>
    public decimal Subtotal => Money.Round(Items.Sum(i => i.LineAmount));

    /// <summary>Subtotal plus charges.</summary>
    public decimal Total => Money.Round(Subtotal + Charges.Sum(c => c.Amount));

    /// <summary>Total saved against marked prices.</summary>
    public decimal Saved => Money.Round(Items.Sum(i => i.Saved));
}
