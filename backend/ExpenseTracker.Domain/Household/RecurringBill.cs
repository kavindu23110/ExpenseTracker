using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Household;

/// <summary>A bill that comes every month, such as electricity, water or rent.</summary>
public sealed class RecurringBill
{
    public const string Paid = "paid";
    public const string Overdue = "overdue";
    public const string DueSoon = "dueSoon";
    public const string Upcoming = "upcoming";

    private RecurringBill() { Name = Category = string.Empty; }

    /// <summary>Creates a validated bill; a late day falls on the last day of shorter months.</summary>
    public RecurringBill(string name, decimal amount, int dayOfMonth, string category)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Enter a name for the bill.");
        if (amount <= 0) throw new DomainException("The amount must be above zero.");
        if (dayOfMonth is < 1 or > 31) throw new DomainException("The due day must be from 1 to 31.");
        if (!BillCategory.IsValid(category)) throw new DomainException("Choose a valid category.");
        Name = name.Trim();
        Amount = amount;
        DayOfMonth = dayOfMonth;
        Category = category;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public int DayOfMonth { get; private set; }
    public string Category { get; private set; }
    public DateOnly? LastPaid { get; private set; }

    /// <summary>The due date in the given date's month.</summary>
    public DateOnly DueDate(DateOnly today) =>
        new(today.Year, today.Month, Math.Min(DayOfMonth, DateTime.DaysInMonth(today.Year, today.Month)));

    /// <summary>True when this month's bill has been paid.</summary>
    public bool IsPaid(DateOnly today) => LastPaid is { } p && p.Year == today.Year && p.Month == today.Month;

    /// <summary>Paid, overdue, due within the reminder window, or upcoming.</summary>
    public string Status(DateOnly today, int reminderDays)
    {
        if (IsPaid(today)) return Paid;
        var due = DueDate(today);
        if (due < today) return Overdue;
        return due.DayNumber - today.DayNumber <= reminderDays ? DueSoon : Upcoming;
    }

    /// <summary>Marks this month's bill as paid.</summary>
    public void MarkPaid(DateOnly today)
    {
        if (IsPaid(today)) throw new DomainException($"{Name} is already paid this month.");
        LastPaid = today;
    }
}
