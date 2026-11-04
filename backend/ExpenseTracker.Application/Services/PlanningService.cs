using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Household;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

/// <summary>Budgets, savings goals and recurring bills.</summary>
public sealed class PlanningService(
    IBillRepository billHistory,
    IStore<Bill> billStore,
    IStore<Budget> budgets,
    IStore<SavingsGoal> goals,
    IStore<RecurringBill> recurring,
    CategoryService categories,
    ITodayProvider today,
    IOptions<BillOptions> options)
{
    /// <summary>Builds the planning page for the current month.</summary>
    public async Task<PlanningDto> GetAsync(CancellationToken ct)
    {
        var now = today.Today;
        var limits = (await budgets.ListAsync(ct)).ToDictionary(b => b.Category, b => b.MonthlyLimit);
        var spent = (await billHistory.GetAllAsync(ct)).Where(b => b.Date.Year == now.Year && b.Date.Month == now.Month)
            .GroupBy(b => b.Category).ToDictionary(g => g.Key, g => g.Sum(b => b.Amount));
        var all = await categories.AllAsync(ct);
        var rows = all.Select(c =>
        {
            var used = spent.GetValueOrDefault(c);
            decimal? limit = limits.TryGetValue(c, out var l) ? l : null;
            return new BudgetRowDto(c, limit, used, limit is { } m ? Percent(used, m) : 0, limit is { } max && used > max);
        }).ToList();
        var goalRows = (await goals.ListAsync(ct)).OrderBy(g => g.DueDate)
            .Select(g => new SavingsGoalDto(g.Id, g.Name, g.Target, g.DueDate.ToString("yyyy-MM-dd"), g.Saved, g.MonthlyNeeded(now), Percent(g.Saved, g.Target))).ToList();
        var reminderDays = options.Value.ReminderDays;
        var billRows = (await recurring.ListAsync(ct)).OrderBy(b => b.DueDate(now))
            .Select(b => new RecurringBillDto(b.Id, b.Name, b.Amount, b.DayOfMonth, b.Category, b.DueDate(now).ToString("yyyy-MM-dd"), b.Status(now, reminderDays))).ToList();
        return new PlanningDto(now.ToString("yyyy-MM"), all, rows, goalRows, billRows,
            billRows.Count(b => b.Status is RecurringBill.Overdue or RecurringBill.DueSoon), await categories.CustomAsync(ct));
    }

    /// <summary>Sets or removes a category's budget.</summary>
    public async Task SetBudgetAsync(SetBudgetRequest r, CancellationToken ct)
    {
        var existing = await budgets.FindAsync(r.Category ?? "", ct);
        if (r.Limit is null)
        {
            if (existing is not null) budgets.Remove(existing);
        }
        else if (existing is null)
        {
            await categories.RequireAsync(r.Category, ct);
            budgets.Add(new Budget(r.Category!, r.Limit.Value));
        }
        else existing.ChangeLimit(r.Limit.Value);
        await budgets.SaveAsync(ct);
    }

    /// <summary>Creates a savings goal.</summary>
    public async Task AddGoalAsync(AddGoalRequest r, CancellationToken ct)
    {
        if (!DateOnly.TryParseExact(r.DueDate, "yyyy-MM-dd", out var due)) throw new DomainException("Choose a due date.");
        goals.Add(new SavingsGoal(r.Name, r.Target, due));
        await goals.SaveAsync(ct);
    }

    /// <summary>Adds to or takes from a goal; false when it does not exist.</summary>
    public async Task<bool> AddSavingAsync(int id, decimal amount, CancellationToken ct)
    {
        if (await goals.FindAsync(id, ct) is not { } goal) return false;
        goal.AddSaving(amount);
        await goals.SaveAsync(ct);
        return true;
    }

    /// <summary>Deletes a goal; false when it does not exist.</summary>
    public async Task<bool> RemoveGoalAsync(int id, CancellationToken ct)
    {
        if (await goals.FindAsync(id, ct) is not { } goal) return false;
        goals.Remove(goal);
        await goals.SaveAsync(ct);
        return true;
    }

    /// <summary>Creates a recurring bill.</summary>
    public async Task AddRecurringAsync(AddRecurringBillRequest r, CancellationToken ct)
    {
        await categories.RequireAsync(r.Category, ct);
        recurring.Add(new RecurringBill(r.Name, r.Amount, r.DayOfMonth, r.Category));
        await recurring.SaveAsync(ct);
    }

    /// <summary>Deletes a recurring bill; expenses already logged stay. False when it does not exist.</summary>
    public async Task<bool> RemoveRecurringAsync(int id, CancellationToken ct)
    {
        if (await recurring.FindAsync(id, ct) is not { } bill) return false;
        recurring.Remove(bill);
        await recurring.SaveAsync(ct);
        return true;
    }

    /// <summary>Marks this month's bill paid and logs it as an expense; false when it does not exist.</summary>
    public async Task<bool> PayAsync(int id, decimal? amount, CancellationToken ct)
    {
        if (await recurring.FindAsync(id, ct) is not { } bill) return false;
        var paid = amount ?? bill.Amount;
        var now = today.Today;
        bill.MarkPaid(now);
        var content = new BillContent(BillLanguage.English, bill.Name, null, null, null, null, now, null, null, null, null, [new BillItem(bill.Name, 1, paid)], []);
        billStore.Add(Bill.Create(content, bill.Name, now, paid, bill.Category, scanned: false, null));
        await recurring.SaveAsync(ct);
        return true;
    }

    private static int Percent(decimal part, decimal whole) => whole == 0 ? 0 : (int)Math.Round(part / whole * 100, MidpointRounding.AwayFromZero);
}
