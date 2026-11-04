using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Categorisation;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence;

/// <inheritdoc />
public sealed class BillRepository(AppDbContext db) : IBillRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Bill>> GetAllAsync(CancellationToken ct) =>
        await db.Bills.AsNoTracking().AsSplitQuery()
            .OrderByDescending(b => b.Date).ThenByDescending(b => b.Time).ThenByDescending(b => b.Id)
            .ToListAsync(ct);

    /// <inheritdoc />
    public Task<Bill?> FindAsync(int id, CancellationToken ct) =>
        db.Bills.AsNoTracking().AsSplitQuery().FirstOrDefaultAsync(b => b.Id == id, ct);

    /// <inheritdoc />
    public Task<BillFile?> FindFileAsync(int billId, CancellationToken ct) =>
        db.BillFiles.AsNoTracking().FirstOrDefaultAsync(f => f.BillId == billId, ct);

    /// <inheritdoc />
    public async Task<string?> FindLearnedCategoryAsync(string merchant, CancellationToken ct) =>
        (await db.LearnedMerchants.AsNoTracking().FirstOrDefaultAsync(m => m.Merchant == merchant, ct))?.Category;

    /// <inheritdoc />
    public async Task AddAsync(Bill bill, string? categoryToRemember, CancellationToken ct)
    {
        db.Bills.Add(bill);
        if (categoryToRemember is not null) await RememberAsync(bill.Merchant, categoryToRemember, ct);
        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, Action<Bill> change, CancellationToken ct)
    {
        var bill = await db.Bills.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (bill is null) return false;
        change(bill);
        await db.SaveChangesAsync(ct);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ChangeCategoryAsync(int id, string category, CancellationToken ct)
    {
        var bill = await db.Bills.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (bill is null) return false;
        bill.ChangeCategory(category);
        await RememberAsync(bill.Merchant, category, ct);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task RememberAsync(string merchant, string category, CancellationToken ct)
    {
        var learned = await db.LearnedMerchants.FirstOrDefaultAsync(m => m.Merchant == merchant, ct);
        if (learned is null) db.LearnedMerchants.Add(new LearnedMerchant(merchant, category));
        else learned.ChangeTo(category);
    }
}
