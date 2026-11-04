namespace ExpenseTracker.Domain.Bills;

/// <summary>The user's answer on whether two item names are the same product.</summary>
public sealed class ItemMapping
{
    private ItemMapping() { KeyA = KeyB = string.Empty; }

    /// <summary>Creates a decision; the pair is stored in a fixed order.</summary>
    public ItemMapping(string first, string second, bool same)
    {
        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second) || first == second) throw new DomainException("Choose two different items.");
        (KeyA, KeyB) = Order(first, second);
        Same = same;
    }

    public string KeyA { get; private set; }
    public string KeyB { get; private set; }
    public bool Same { get; private set; }

    /// <summary>Puts a pair in the order it is stored.</summary>
    public static (string, string) Order(string first, string second) =>
        string.CompareOrdinal(first, second) <= 0 ? (first, second) : (second, first);

    /// <summary>Records a changed answer.</summary>
    public void ChangeTo(bool same) => Same = same;
}
