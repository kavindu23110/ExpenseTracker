using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Application.Ports;

/// <summary>Stores bills and remembered shop categories.</summary>
public interface IBillRepository
{
    /// <summary>Loads every bill with items and charges, without file data.</summary>
    Task<IReadOnlyList<Bill>> GetAllAsync(CancellationToken ct);

    /// <summary>Loads one bill with items and charges, without file data.</summary>
    Task<Bill?> FindAsync(int id, CancellationToken ct);

    /// <summary>Loads a bill's original file.</summary>
    Task<BillFile?> FindFileAsync(int billId, CancellationToken ct);

    /// <summary>Returns the remembered category for a shop.</summary>
    Task<string?> FindLearnedCategoryAsync(string merchant, CancellationToken ct);

    /// <summary>Saves a bill and, when given, the category to remember for its shop.</summary>
    Task AddAsync(Bill bill, string? categoryToRemember, CancellationToken ct);

    /// <summary>Saves a change to a bill; false when there is no such bill.</summary>
    Task<bool> UpdateAsync(int id, Action<Bill> change, CancellationToken ct);

    /// <summary>Changes a saved bill's category and remembers it for the shop; false when there is no such bill.</summary>
    Task<bool> ChangeCategoryAsync(int id, string category, CancellationToken ct);
}
