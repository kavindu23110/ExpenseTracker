using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Services;

/// <summary>Turns uploads into drafts for review.</summary>
public interface IBillScanService
{
    /// <summary>Reads an uploaded file into a draft.</summary>
    Task<BillDraftDto> ScanAsync(UploadedFile file, string? langHint, CancellationToken ct);
}
