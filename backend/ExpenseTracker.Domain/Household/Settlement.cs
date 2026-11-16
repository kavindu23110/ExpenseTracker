namespace ExpenseTracker.Domain.Household;

/// <summary>Money one member gave another to even up shared spending.</summary>
public sealed class Settlement
{
    private Settlement() { From = To = string.Empty; }

    /// <summary>Records a payment between two different members.</summary>
    public Settlement(string from, string to, decimal amount, DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to)) throw new DomainException("Choose who paid and who received.");
        if (string.Equals(from.Trim(), to.Trim(), StringComparison.OrdinalIgnoreCase)) throw new DomainException("Choose two different members.");
        if (amount <= 0) throw new DomainException("The amount must be above zero.");
        From = from.Trim();
        To = to.Trim();
        Amount = amount;
        Date = date;
    }

    public int Id { get; private set; }
    public string From { get; private set; }
    public string To { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
}
