using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Household;

/// <summary>One member's position: above zero means the others owe them.</summary>
public sealed record MemberBalance(string Member, decimal Paid, decimal Share, decimal Balance);

/// <summary>A payment that would even everyone up.</summary>
public sealed record SuggestedPayment(string From, string To, decimal Amount);

/// <summary>Works out who owes whom; bills with a payer are shared equally by all members.</summary>
public static class HouseholdBalance
{
    /// <summary>Each member's paid amount, share and balance after settlements.</summary>
    public static IReadOnlyList<MemberBalance> Compute(IReadOnlyList<string> members, IEnumerable<Bill> bills, IEnumerable<Settlement> settlements)
    {
        if (members.Count == 0) return [];
        var paidBills = bills.Where(b => b.PaidBy is not null && members.Contains(b.PaidBy, StringComparer.OrdinalIgnoreCase)).ToList();
        var share = Money.Round(paidBills.Sum(b => b.Amount) / members.Count);
        var list = settlements.ToList();
        return members.Select(m =>
        {
            var paid = paidBills.Where(b => Same(b.PaidBy!, m)).Sum(b => b.Amount);
            var sent = list.Where(s => Same(s.From, m)).Sum(s => s.Amount);
            var received = list.Where(s => Same(s.To, m)).Sum(s => s.Amount);
            return new MemberBalance(m, Money.Round(paid), share, Money.Round(paid - share + sent - received));
        }).ToList();
    }

    /// <summary>Simple payments that clear the balances, largest debts first.</summary>
    public static IReadOnlyList<SuggestedPayment> SettleUp(IReadOnlyList<MemberBalance> balances)
    {
        var owe = balances.Where(b => b.Balance < 0).Select(b => (b.Member, Left: -b.Balance)).OrderByDescending(b => b.Left).ToList();
        var due = balances.Where(b => b.Balance > 0).Select(b => (b.Member, Left: b.Balance)).OrderByDescending(b => b.Left).ToList();
        var payments = new List<SuggestedPayment>();
        int i = 0, j = 0;
        while (i < owe.Count && j < due.Count)
        {
            var amount = Math.Min(owe[i].Left, due[j].Left);
            if (amount >= 1) payments.Add(new SuggestedPayment(owe[i].Member, due[j].Member, Money.Round(amount)));
            owe[i] = (owe[i].Member, owe[i].Left - amount);
            due[j] = (due[j].Member, due[j].Left - amount);
            if (owe[i].Left <= 0.005m) i++;
            if (due[j].Left <= 0.005m) j++;
        }
        return payments;
    }

    private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
