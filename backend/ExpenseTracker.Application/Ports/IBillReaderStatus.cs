namespace ExpenseTracker.Application.Ports;

/// <summary>Says whether real bill reading is configured.</summary>
public interface IBillReaderStatus
{
    /// <summary>True when uploads can be read.</summary>
    bool IsAvailable { get; }
}
