namespace ExpenseTracker.Application;

/// <summary>Tunable limits and thresholds, bound from the Bills configuration section.</summary>
public sealed class BillOptions
{
    public const string Section = "Bills";

    public int ReminderDays { get; set; } = 3;

    public long MaxUploadBytes { get; set; } = 10 * 1024 * 1024;
    public decimal AboveMarketTolerancePercent { get; set; } = 5;
    public decimal TotalMismatchTolerance { get; set; } = 1;
    public string TimeZone { get; set; } = "Asia/Colombo";
}
