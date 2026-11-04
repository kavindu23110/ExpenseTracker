namespace ExpenseTracker.Application.Services;

/// <summary>Gives today's date in the app's time zone.</summary>
public interface ITodayProvider
{
    /// <summary>Today's date.</summary>
    DateOnly Today { get; }
}
