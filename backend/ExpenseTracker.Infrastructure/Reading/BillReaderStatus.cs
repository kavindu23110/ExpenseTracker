using ExpenseTracker.Application.Ports;

namespace ExpenseTracker.Infrastructure.Reading;

/// <inheritdoc />
public sealed record BillReaderStatus(bool IsAvailable) : IBillReaderStatus;
