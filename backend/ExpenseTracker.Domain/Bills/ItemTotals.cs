using System.Text.RegularExpressions;

namespace ExpenseTracker.Domain.Bills;

/// <summary>One product's combined quantity and spend, with the item names merged into it.</summary>
public sealed record ItemTotal(string Name, string? NameSi, decimal Qty, string? Unit, decimal Amount, IReadOnlyList<string> Keys, IReadOnlyList<string> Names);

/// <summary>Two item names that may be the same product, for the user to decide.</summary>
public sealed record ItemQuestion(string KeyA, string NameA, string KeyB, string NameB);

/// <summary>Combines line items for the same product, keeping different units apart.</summary>
public static partial class ItemTotals
{
    private const int MaxKeyLength = 200;
    private const double SimilarNames = 0.5;

    /// <summary>The identity of an item: its bill name without case or punctuation.</summary>
    public static string KeyOf(BillItem item) => KeyOf(item.NameSi ?? item.Name);

    /// <summary>The identity of an item name.</summary>
    public static string KeyOf(string name)
    {
        var key = Normalise(name);
        return key.Length > MaxKeyLength ? key[..MaxKeyLength] : key;
    }

    /// <summary>Gives the function that identifies a product: its confirmed mapping and its unit.</summary>
    public static Func<BillItem, (string, string?)> ProductKey(IReadOnlyCollection<ItemMapping>? mappings)
    {
        var roots = Roots(mappings ?? []);
        return i => (Root(roots, KeyOf(i)), i.Unit?.ToLowerInvariant());
    }

    /// <summary>Totals the items by product and unit, largest spend first; confirmed mappings merge names.</summary>
    public static IReadOnlyList<ItemTotal> Summarise(IEnumerable<BillItem> items, IReadOnlyCollection<ItemMapping>? mappings = null)
    {
        return items.GroupBy(ProductKey(mappings))
            .Select(g =>
            {
                var main = g.GroupBy(KeyOf).OrderByDescending(n => n.Sum(i => i.LineAmount)).First().First();
                var keys = g.Select(KeyOf).Distinct().Order(StringComparer.Ordinal).ToList();
                var names = g.GroupBy(KeyOf).OrderBy(n => n.Key, StringComparer.Ordinal).Select(n => n.First().Name).ToList();
                return new ItemTotal(main.Name, main.NameSi, g.Sum(i => i.Qty), main.Unit, g.Sum(i => i.LineAmount), keys, names);
            })
            .OrderByDescending(t => t.Amount)
            .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Finds similar names sold in the same unit that the user has not decided on.</summary>
    public static IReadOnlyList<ItemQuestion> FindQuestions(IEnumerable<BillItem> items, IReadOnlyCollection<ItemMapping>? mappings = null)
    {
        var decided = mappings ?? [];
        var roots = Roots(decided);
        var answered = decided.Select(m => (m.KeyA, m.KeyB)).ToHashSet();
        var names = items.GroupBy(i => (Key: KeyOf(i), Unit: i.Unit?.ToLowerInvariant()))
            .Select(g => (g.Key.Key, g.Key.Unit, g.First().Name))
            .OrderBy(n => n.Key, StringComparer.Ordinal)
            .ToList();

        var questions = new List<ItemQuestion>();
        for (var a = 0; a < names.Count; a++)
        {
            for (var b = a + 1; b < names.Count; b++)
            {
                var (x, y) = (names[a], names[b]);
                if (x.Unit != y.Unit || x.Key == y.Key || Root(roots, x.Key) == Root(roots, y.Key)) continue;
                if (answered.Contains(ItemMapping.Order(x.Key, y.Key)) || !AreSimilar(x.Name, y.Name)) continue;
                questions.Add(new ItemQuestion(x.Key, x.Name, y.Key, y.Name));
            }
        }

        return questions;
    }

    private static bool AreSimilar(string first, string second)
    {
        var a = Normalise(first).Split(' ').ToHashSet();
        var b = Normalise(second).Split(' ').ToHashSet();
        var shared = a.Intersect(b).ToList();
        if (!shared.Any(w => w.Length >= 3 && !w.All(char.IsDigit))) return false;
        return shared.Count == Math.Min(a.Count, b.Count) || (double)shared.Count / a.Union(b).Count() >= SimilarNames;
    }

    private static Dictionary<string, string> Roots(IEnumerable<ItemMapping> mappings)
    {
        var roots = new Dictionary<string, string>();
        foreach (var m in mappings.Where(m => m.Same))
        {
            var (a, b) = (Root(roots, m.KeyA), Root(roots, m.KeyB));
            if (a != b) roots[b] = a;
        }

        return roots;
    }

    private static string Root(Dictionary<string, string> roots, string key)
    {
        while (roots.TryGetValue(key, out var parent)) key = parent;
        return key;
    }

    private static string Normalise(string name) => NotPartOfWord().Replace(name.ToLowerInvariant(), " ").Trim();

    [GeneratedRegex(@"[^\p{L}\p{M}\p{N}]+")]
    private static partial Regex NotPartOfWord();
}
