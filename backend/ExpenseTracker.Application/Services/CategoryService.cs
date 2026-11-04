using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Household;

namespace ExpenseTracker.Application.Services;

/// <summary>The built-in categories plus those the household added.</summary>
public sealed class CategoryService(IStore<CustomCategory> custom, IBillRepository bills, IStore<Budget> budgets, IStore<RecurringBill> recurring)
{
    /// <summary>Lists the household's own categories by name.</summary>
    public async Task<IReadOnlyList<string>> CustomAsync(CancellationToken ct) =>
        (await custom.ListAsync(ct)).Select(c => c.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Lists every category, built-in ones first and Other last.</summary>
    public async Task<IReadOnlyList<string>> AllAsync(CancellationToken ct) =>
        [.. BillCategory.All.Where(c => c != BillCategory.Other), .. await CustomAsync(ct), BillCategory.Other];

    /// <summary>Rejects a category that does not exist.</summary>
    public async Task RequireAsync(string? category, CancellationToken ct)
    {
        if (category is null || !(await AllAsync(ct)).Contains(category)) throw new DomainException($"'{category}' is not a category.");
    }

    /// <summary>Adds a category, refusing a name already in use.</summary>
    public async Task AddAsync(string? name, CancellationToken ct)
    {
        var category = new CustomCategory(name);
        if ((await AllAsync(ct)).Contains(category.Name, StringComparer.OrdinalIgnoreCase)) throw new DomainException($"{category.Name} is already a category.");
        custom.Add(category);
        await custom.SaveAsync(ct);
    }

    /// <summary>Removes a category nothing uses; false when it does not exist.</summary>
    public async Task<bool> RemoveAsync(string name, CancellationToken ct)
    {
        if (await custom.FindAsync(name, ct) is not { } category) return false;
        var inUse = (await bills.GetAllAsync(ct)).Any(b => b.Category == category.Name)
            || (await budgets.ListAsync(ct)).Any(b => b.Category == category.Name)
            || (await recurring.ListAsync(ct)).Any(b => b.Category == category.Name);
        if (inUse) throw new DomainException($"{category.Name} is used by a bill, budget or recurring bill, so it can't be removed.");
        custom.Remove(category);
        await custom.SaveAsync(ct);
        return true;
    }
}
