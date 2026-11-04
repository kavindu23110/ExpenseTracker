namespace ExpenseTracker.Domain.Categorisation;

/// <summary>The category a user chose for a shop, used next time.</summary>
public sealed class LearnedMerchant
{
    private LearnedMerchant() { Merchant = string.Empty; Category = string.Empty; }

    /// <summary>Remembers a shop's category.</summary>
    public LearnedMerchant(string merchant, string category)
    {
        Merchant = merchant;
        Category = category;
    }

    public string Merchant { get; private set; }
    public string Category { get; private set; }

    /// <summary>Replaces the remembered category.</summary>
    public void ChangeTo(string category) => Category = category;
}
