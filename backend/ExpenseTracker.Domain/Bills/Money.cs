namespace ExpenseTracker.Domain.Bills;

/// <summary>Rounds money the way bills print it.</summary>
public static class Money
{
    /// <summary>Rounds to two decimals, halves away from zero.</summary>
    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
