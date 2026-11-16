using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Household;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

/// <summary>Budgets running out, bills due and food about to expire, in the app and as a daily WhatsApp message.</summary>
public sealed class AlertService(
    PlanningService planning,
    IStore<PantryItem> pantry,
    IStore<AppUser> users,
    IStore<AlertDigest> digests,
    WhatsAppConfigService whatsAppConfig,
    IWhatsAppClient whatsApp,
    ITodayProvider today,
    IOptions<BillOptions> options,
    ILogger<AlertService> logger)
{
    /// <summary>A budget this far used gets a warning.</summary>
    public const int BudgetWarnPercent = 90;

    /// <summary>Everything needing attention today.</summary>
    public async Task<IReadOnlyList<AlertDto>> GetAsync(CancellationToken ct)
    {
        var plan = await planning.GetAsync(ct);
        var alerts = new List<AlertDto>();
        foreach (var b in plan.Budgets.Where(b => b.Limit is not null && (b.Over || b.Percent >= BudgetWarnPercent)))
            alerts.Add(new("budget", b.Over ? $"{b.Category} is over budget: Rs {b.Spent:N0} of Rs {b.Limit:N0}." : $"{b.Category} has used {b.Percent}% of its budget."));
        foreach (var r in plan.Bills.Where(b => b.Status is RecurringBill.Overdue or RecurringBill.DueSoon))
            alerts.Add(new("bill", r.Status == RecurringBill.Overdue ? $"{r.Name} (Rs {r.Amount:N0}) was due on {r.DueDate}." : $"{r.Name} (Rs {r.Amount:N0}) is due on {r.DueDate}."));
        var soon = today.Today.AddDays(PantryService.ExpirySoonDays);
        foreach (var p in (await pantry.ListAsync(ct)).Where(p => p.ExpiresBy(soon)).OrderBy(p => p.Expiry))
            alerts.Add(new("expiry", p.Expiry < today.Today ? $"{p.Name} expired on {p.Expiry:yyyy-MM-dd}." : $"{p.Name} expires on {p.Expiry:yyyy-MM-dd}."));
        return alerts;
    }

    /// <summary>Sends today's alerts once to every member with a linked number, after the morning hour.</summary>
    public async Task SendDailyAsync(int localHour, CancellationToken ct)
    {
        if (localHour < options.Value.AlertHour) return;
        var day = today.Today.ToString("yyyy-MM-dd");
        if (await digests.FindAsync(day, ct) is not null) return;
        if (!(await whatsAppConfig.GetAsync(ct)).IsConfigured) return;
        var alerts = await GetAsync(ct);
        digests.Add(new AlertDigest(day));
        await digests.SaveAsync(ct);
        if (alerts.Count == 0) return;
        var text = "Expense Tracker reminders:\n" + string.Join("\n", alerts.Select(a => "• " + a.Text));
        foreach (var number in (await users.ListAsync(ct)).Select(u => u.WhatsAppNumber).OfType<string>())
        {
            try { await whatsApp.SendTextAsync(number, text, ct); }
            catch (Exception e) when (e is not OperationCanceledException) { logger.LogWarning(e, "A reminder could not be sent on WhatsApp."); }
        }
    }
}
