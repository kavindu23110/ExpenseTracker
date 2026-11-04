namespace ExpenseTracker.Domain.Household;

/// <summary>Records that the shopping list was filled from last month's bills for a month.</summary>
public sealed class ToBuyMonthlyFill
{
    private ToBuyMonthlyFill() { Month = string.Empty; }

    /// <summary>Creates the record for a month, as yyyy-MM.</summary>
    public ToBuyMonthlyFill(string month) => Month = month;

    public string Month { get; private set; }
}
