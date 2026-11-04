namespace ExpenseTracker.Domain.PriceWatch;

/// <summary>A published year-on-year rate; Category is null for the headline figure.</summary>
public sealed class InflationRate
{
    private InflationRate() { Label = string.Empty; }

    /// <summary>Creates a rate; a null previous rate keeps it off the summary cards.</summary>
    public InflationRate(int sort, string label, string? category, decimal rate, decimal? previousRate, string? note = null)
    {
        Sort = sort;
        Label = label;
        Category = category;
        Rate = rate;
        PreviousRate = previousRate;
        Note = note;
    }

    public int Id { get; private set; }
    public int Sort { get; private set; }
    public string Label { get; private set; }
    public string? Category { get; private set; }
    public decimal Rate { get; private set; }
    public decimal? PreviousRate { get; private set; }
    public string? Note { get; private set; }
}
