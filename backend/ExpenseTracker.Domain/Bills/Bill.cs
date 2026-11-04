namespace ExpenseTracker.Domain.Bills;

/// <summary>A filed bill; the aggregate root for its items, charges and original file.</summary>
public sealed class Bill
{
    private readonly List<BillItem> _items = [];
    private readonly List<BillCharge> _charges = [];

    private Bill()
    {
        Lang = BillLanguage.English;
        Merchant = string.Empty;
        Category = BillCategory.Other;
    }

    public int Id { get; private set; }
    public string Lang { get; private set; }
    public string Merchant { get; private set; }
    public string? MerchantSi { get; private set; }
    public string? Address { get; private set; }
    public string? AddressSi { get; private set; }
    public string? BillNo { get; private set; }
    public string Category { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly? Time { get; private set; }
    public string? Payment { get; private set; }
    public string? PaymentSi { get; private set; }
    public decimal? Paid { get; private set; }
    public decimal Amount { get; private set; }
    public bool Scanned { get; private set; }
    public string? FileName { get; private set; }
    public bool HasFile { get; private set; }
    public BillFile? File { get; private set; }
    public IReadOnlyList<BillItem> Items => _items;
    public IReadOnlyList<BillCharge> Charges => _charges;

    /// <summary>Moves the bill to another category.</summary>
    public void ChangeCategory(string category)
    {
        if (!BillCategory.IsValid(category)) throw new DomainException($"'{category}' is not a category.");
        Category = category;
    }

    /// <summary>Same shop, date and total, unless a bill number or time recorded on both differs.</summary>
    public bool IsDuplicateOf(Bill other) =>
        string.Equals(Merchant, other.Merchant, StringComparison.OrdinalIgnoreCase)
        && Date == other.Date
        && Amount == other.Amount
        && (string.IsNullOrWhiteSpace(BillNo) || string.IsNullOrWhiteSpace(other.BillNo) || string.Equals(BillNo.Trim(), other.BillNo.Trim(), StringComparison.OrdinalIgnoreCase))
        && (Time is null || other.Time is null || Time == other.Time);

    /// <summary>Files a bill after checking the required fields.</summary>
    public static Bill Create(BillContent content, string merchant, DateOnly? date, decimal amount, string category, bool scanned, string? fileName, BillFile? file = null)
    {
        if (!BillLanguage.IsValid(content.Lang)) throw new DomainException("Bill language must be English or Sinhala.");
        if (string.IsNullOrWhiteSpace(merchant)) throw new DomainException("Enter the shop name.");
        if (date is null) throw new DomainException("Enter the bill date.");
        if (amount < 0) throw new DomainException("The total cannot be negative.");
        if (!BillCategory.IsValid(category)) throw new DomainException($"'{category}' is not a category.");
        if (content.Items.Count == 0) throw new DomainException("A bill needs at least one line item.");

        var bill = new Bill
        {
            Lang = content.Lang,
            Merchant = merchant.Trim(),
            MerchantSi = content.MerchantSi,
            Address = content.Address,
            AddressSi = content.AddressSi,
            BillNo = content.BillNo,
            Category = category,
            Date = date.Value,
            Time = content.Time,
            Payment = content.Payment,
            PaymentSi = content.PaymentSi,
            Paid = content.Paid,
            Amount = Money.Round(amount),
            Scanned = scanned,
            FileName = fileName,
            HasFile = file is not null,
            File = file,
        };
        bill._items.AddRange(content.Items);
        bill._charges.AddRange(content.Charges);
        return bill;
    }

    /// <summary>Sum of the line items.</summary>
    public decimal Subtotal => Money.Round(_items.Sum(i => i.LineAmount));

    /// <summary>Total saved against marked prices.</summary>
    public decimal Saved => Money.Round(_items.Sum(i => i.Saved));

    /// <summary>Change returned, when the cash given is known.</summary>
    public decimal? Change => Paid is null ? null : Money.Round(Paid.Value - Amount);
}
