using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Application.Services;

/// <inheritdoc />
public sealed class ItemMappingService(IItemMappingRepository mappings) : IItemMappingService
{
    /// <inheritdoc />
    public Task DecideAsync(ItemMappingRequest request, CancellationToken ct)
    {
        var keys = (request.Keys ?? []).Distinct().ToList();
        if (keys.Count < 2) throw new DomainException("Choose at least two items.");
        var pairs = keys.SelectMany((a, i) => keys.Skip(i + 1).Select(b => new ItemMapping(a, b, request.Same))).ToList();
        return mappings.SaveAsync(pairs, ct);
    }
}
