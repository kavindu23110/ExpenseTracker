using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Household;

namespace ExpenseTracker.Application.Services;

/// <summary>Who paid what, and who owes whom.</summary>
public sealed class BalanceService(IBillRepository bills, IStore<AppUser> users, IStore<Settlement> settlements, ITodayProvider today)
{
    /// <summary>Each member's balance, the payments that would clear them, and recent settle-ups.</summary>
    public async Task<BalancesDto> GetAsync(CancellationToken ct)
    {
        var members = (await users.ListAsync(ct)).Select(u => u.Name).OrderBy(n => n).ToList();
        var paid = await settlements.ListAsync(ct);
        var balances = HouseholdBalance.Compute(members, await bills.GetAllAsync(ct), paid);
        return new(
            balances.Select(b => new MemberBalanceDto(b.Member, b.Paid, b.Share, b.Balance)).ToList(),
            HouseholdBalance.SettleUp(balances).Select(p => new SuggestedPaymentDto(p.From, p.To, p.Amount)).ToList(),
            paid.OrderByDescending(s => s.Date).ThenByDescending(s => s.Id).Take(20).Select(s => new SettlementDto(s.Id, s.From, s.To, s.Amount, s.Date.ToString("yyyy-MM-dd"))).ToList());
    }

    /// <summary>Records money given from one member to another.</summary>
    public async Task AddAsync(AddSettlementRequest r, CancellationToken ct)
    {
        var names = (await users.ListAsync(ct)).Select(u => u.Name).ToList();
        string Member(string? n) => names.FirstOrDefault(x => string.Equals(x, n?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? throw new DomainException($"{n} is not a household member.");
        settlements.Add(new Settlement(Member(r.From), Member(r.To), r.Amount, today.Today));
        await settlements.SaveAsync(ct);
    }

    /// <summary>Deletes a recorded settle-up. False when missing.</summary>
    public async Task<bool> RemoveAsync(int id, CancellationToken ct)
    {
        if (await settlements.FindAsync(id, ct) is not { } s) return false;
        settlements.Remove(s);
        await settlements.SaveAsync(ct);
        return true;
    }
}
