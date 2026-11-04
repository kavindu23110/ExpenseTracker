using ExpenseTracker.Application.Models;
using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Application.Services;

/// <summary>Files checked bills and retrieves them.</summary>
public interface IBillFilingService
{
    /// <summary>Saves a checked draft and returns its id.</summary>
    Task<int> SaveAsync(SaveBillRequest request, UploadedFile? file, CancellationToken ct);

    /// <summary>Changes a saved bill's category; false when there is no such bill.</summary>
    Task<bool> ChangeCategoryAsync(int id, string? category, CancellationToken ct);

    /// <summary>Returns one bill's details.</summary>
    Task<BillDetailDto?> GetAsync(int id, CancellationToken ct);

    /// <summary>Returns a bill's original file.</summary>
    Task<BillFile?> GetFileAsync(int id, CancellationToken ct);
}
