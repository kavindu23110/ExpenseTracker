using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Household;

/// <summary>A monthly spending limit for one category.</summary>
public sealed class Budget
{
    private Budget() { Category = string.Empty; }

    /// <summary>Creates a validated limit.</summary>
    public Budget(string category, decimal monthlyLimit)
    {
        if (!BillCategory.IsValid(category)) throw new DomainException("Choose a valid category.");
        Category = category;
        ChangeLimit(monthlyLimit);
    }

    public string Category { get; private set; }
    public decimal MonthlyLimit { get; private set; }

    /// <summary>Changes the limit.</summary>
    public void ChangeLimit(decimal monthlyLimit)
    {
        if (monthlyLimit <= 0) throw new DomainException("The budget must be above zero.");
        MonthlyLimit = monthlyLimit;
    }
}
