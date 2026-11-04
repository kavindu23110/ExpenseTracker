using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Household;

/// <summary>Money set aside for an irregular cost, such as school fees or insurance.</summary>
public sealed class SavingsGoal
{
    private SavingsGoal() { Name = string.Empty; }

    /// <summary>Creates a validated goal.</summary>
    public SavingsGoal(string name, decimal target, DateOnly dueDate)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Enter a name for the goal.");
        if (target <= 0) throw new DomainException("The target must be above zero.");
        Name = name.Trim();
        Target = target;
        DueDate = dueDate;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public decimal Target { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal Saved { get; private set; }

    /// <summary>Adds to the amount saved; a negative amount takes money back out.</summary>
    public void AddSaving(decimal amount)
    {
        if (amount == 0) throw new DomainException("Enter an amount.");
        if (Saved + amount < 0) throw new DomainException("You cannot take out more than is saved.");
        Saved += amount;
    }

    /// <summary>What to save each month to reach the target, counting this month.</summary>
    public decimal MonthlyNeeded(DateOnly today)
    {
        var months = Math.Max(1, (DueDate.Year - today.Year) * 12 + DueDate.Month - today.Month + 1);
        return Money.Round(Math.Max(0, Target - Saved) / months);
    }
}
