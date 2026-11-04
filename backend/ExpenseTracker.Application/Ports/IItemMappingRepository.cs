using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Application.Ports;

/// <summary>Stores the user's answers about which item names match.</summary>
public interface IItemMappingRepository
{
    /// <summary>Loads every answer.</summary>
    Task<IReadOnlyList<ItemMapping>> GetAllAsync(CancellationToken ct);

    /// <summary>Saves answers, replacing earlier ones for the same pairs.</summary>
    Task SaveAsync(IReadOnlyList<ItemMapping> mappings, CancellationToken ct);
}
