namespace ExpenseTracker.Domain.Household;

/// <summary>Marks that a day's reminders were sent, so they go out once.</summary>
public sealed class AlertDigest
{
    private AlertDigest() { Day = string.Empty; }

    /// <summary>Marks a day, as yyyy-MM-dd.</summary>
    public AlertDigest(string day) { Day = day; }

    public string Day { get; private set; }
}
