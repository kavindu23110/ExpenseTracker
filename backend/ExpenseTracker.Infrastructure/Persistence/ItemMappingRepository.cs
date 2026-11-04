using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Bills;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence;

/// <inheritdoc />
public sealed class ItemMappingRepository(AppDbContext db) : IItemMappingRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ItemMapping>> GetAllAsync(CancellationToken ct) =>
        await db.ItemMappings.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc />
    public async Task SaveAsync(IReadOnlyList<ItemMapping> mappings, CancellationToken ct)
    {
        foreach (var mapping in mappings)
        {
            var existing = await db.ItemMappings.FindAsync([mapping.KeyA, mapping.KeyB], ct);
            if (existing is null) db.ItemMappings.Add(mapping);
            else existing.ChangeTo(mapping.Same);
        }

        await db.SaveChangesAsync(ct);
    }
}
