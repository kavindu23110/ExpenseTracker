using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Services;

/// <summary>Records whether item names are the same product.</summary>
public interface IItemMappingService
{
    /// <summary>Saves the answer for every pair of the given items.</summary>
    Task DecideAsync(ItemMappingRequest request, CancellationToken ct);
}
