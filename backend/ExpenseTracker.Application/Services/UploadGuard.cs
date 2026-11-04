using ExpenseTracker.Application.Models;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Application.Services;

/// <summary>Checks an upload's size and real type.</summary>
public static class UploadGuard
{
    /// <summary>Returns the detected content type or rejects the file.</summary>
    public static string Check(UploadedFile file, BillOptions options)
    {
        if (file.Data.Length == 0) throw new DomainException("That file is empty.");
        if (file.Data.Length > options.MaxUploadBytes) throw new DomainException($"That file is too large. The limit is {options.MaxUploadBytes / (1024 * 1024)} MB.");
        return BillFileType.Detect(file.Data) ?? throw new DomainException("Upload a JPG, PNG, WEBP or PDF file.");
    }
}
