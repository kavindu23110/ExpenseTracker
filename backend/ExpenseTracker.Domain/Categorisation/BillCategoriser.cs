using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Categorisation;

/// <summary>Suggests a category from a remembered shop, else from keyword matches.</summary>
public static class BillCategoriser
{
    private static readonly KeywordRule[] Rules =
    [
        new(BillCategory.Groceries, ["supermarket", "grocery", "rice", "milk", "eggs", "carrots", "dhal", "bread", "bananas"],
            [("සුපර් මාර්කට්", "supermarket"), ("වෙළඳසැල", "shop"), ("ස්ටෝර්ස්", "stores"), ("පිටි", "flour"), ("පපඩම්", "papadam"), ("සහල්", "rice"), ("පරිප්පු", "dhal"), ("පොල්", "coconut"), ("සීනි", "sugar"), ("තේ කොළ", "tea leaves"), ("කිරි", "milk"), ("බිත්තර", "eggs"), ("පාන්", "bread")]),
        new(BillCategory.CreditCard, ["credit card", "card statement", "minimum payment", "minimum amount due"], []),
        new(BillCategory.Dining, ["cafe", "restaurant", "kottu", "latte", "pizza", "burger", "service charge"],
            [("මුළුතැන්ගෙය", "kitchen"), ("කොත්තු", "kottu"), ("බත් සහ ව්‍යංජන", "rice and curry"), ("කෝපි", "coffee"), ("සේවා ගාස්තුව", "service charge")]),
        new(BillCategory.Transport, ["fuel", "petrol", "diesel", "octane", "taxi", "ride", "parking"],
            [("පෙට්‍රල්", "petrol"), ("ඩීසල්", "diesel"), ("ඉන්ධන", "fuel")]),
        new(BillCategory.Utilities, ["electricity", "water", "broadband", "mobile bill", "units"],
            [("විදුලි", "electricity"), ("ජල", "water")]),
        new(BillCategory.Shopping, ["fashion", "clothing", "shoes", "shirt", "sarong", "boutique"],
            [("ඇඳුම්", "clothing"), ("සපත්තු", "shoes"), ("සරම", "sarong")]),
        new(BillCategory.Health, ["pharmacy", "clinic", "hospital", "tablets", "paracetamol", "vitamin"],
            [("ෆාමසිය", "pharmacy"), ("බෙහෙත්", "medicine"), ("පැරසිටමෝල්", "paracetamol"), ("විටමින්", "vitamin")]),
    ];

    /// <summary>Returns the best suggestion; a remembered category always wins.</summary>
    public static CategorySuggestion Suggest(BillContent bill, string? learnedCategory)
    {
        if (learnedCategory is not null)
            return new(learnedCategory, 99, $"you filed {bill.Merchant} under {learnedCategory} before");

        var text = bill.IsSinhala ? SinhalaText(bill) : EnglishText(bill);
        string? best = null;
        List<string> bestHits = [];
        foreach (var rule in Rules)
        {
            var hits = bill.IsSinhala
                ? rule.Sinhala.Where(p => text.Contains(p.Word, StringComparison.Ordinal)).Select(p => $"“{p.Word}” ({p.Gloss})").ToList()
                : rule.Words.Where(w => text.Contains(w, StringComparison.Ordinal)).Select(w => $"“{w}”").ToList();
            if (hits.Count > bestHits.Count)
            {
                best = rule.Category;
                bestHits = hits;
            }
        }

        if (best is null) return new(BillCategory.Other, 40, "no keywords matched, so it needs a quick look");
        var prefix = bill.IsSinhala ? "matched Sinhala words " : "matched ";
        return new(best, Math.Min(98, 72 + bestHits.Count * 6), prefix + string.Join(", ", bestHits.Take(3)));
    }

    private static string SinhalaText(BillContent b) =>
        string.Join(' ', new[] { b.MerchantSi }.Concat(b.Items.Select(i => i.NameSi)).Concat(b.Charges.Select(c => c.LabelSi)).OfType<string>());

    private static string EnglishText(BillContent b) =>
        string.Join(' ', new[] { b.Merchant }.Concat(b.Items.Select(i => i.Name))).ToLowerInvariant();
}
