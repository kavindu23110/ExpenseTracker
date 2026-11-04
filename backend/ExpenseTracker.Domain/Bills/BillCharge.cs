namespace ExpenseTracker.Domain.Bills;

/// <summary>A non-item amount on a bill, such as a service charge or rounding.</summary>
public sealed class BillCharge
{
    private BillCharge() { Label = string.Empty; }

    /// <summary>Creates a charge; negative amounts are allowed for rounding and discounts.</summary>
    public BillCharge(string label, decimal amount, string? labelSi = null)
    {
        if (string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(labelSi)) throw new DomainException("A charge needs a label.");
        Label = string.IsNullOrWhiteSpace(label) ? labelSi!.Trim() : label.Trim();
        LabelSi = string.IsNullOrWhiteSpace(labelSi) ? null : labelSi.Trim();
        Amount = amount;
    }

    public int Id { get; private set; }
    public string Label { get; private set; }
    public string? LabelSi { get; private set; }
    public decimal Amount { get; private set; }
}
