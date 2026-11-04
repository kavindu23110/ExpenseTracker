using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Categorisation;
using ExpenseTracker.Domain.PriceWatch;

namespace ExpenseTracker.Domain.Tests;

/// <summary>Covers the bill, category and price rules ported from the design.</summary>
public class BillRulesTests
{
    private static BillContent Content(string lang, string merchant, string? merchantSi, params BillItem[] items) =>
        new(lang, merchant, merchantSi, null, null, null, new DateOnly(2026, 11, 3), null, null, null, null, items, []);

    [Fact]
    public void Editing_replaces_the_values_and_forgets_an_old_shop_translation()
    {
        var bill = Bill.Create(Content("si", "Shop", "සාප්පුව", new BillItem("Tea", 1, 5)), "Shop", new DateOnly(2026, 11, 1), 5, BillCategory.Other, false, null);

        bill.Edit(" New Shop ", new DateOnly(2026, 10, 2), 40, [new BillItem("Rice", 2, 20)]);

        Assert.Equal("New Shop", bill.Merchant);
        Assert.Null(bill.MerchantSi);
        Assert.Equal(new DateOnly(2026, 10, 2), bill.Date);
        Assert.Equal(40, bill.Amount);
        Assert.Equal("Rice", Assert.Single(bill.Items).Name);
        Assert.Throws<DomainException>(() => bill.Edit("Shop", new DateOnly(2026, 10, 2), 40, []));
    }

    [Fact]
    public void Changing_category_rejects_a_blank_one()
    {
        var bill = Bill.Create(new BillContent(BillLanguage.English, "Shop", null, null, null, null, new DateOnly(2026, 11, 1), null, null, null, null, [new BillItem("Tea", 1, 5)], []), "Shop", new DateOnly(2026, 11, 1), 5, BillCategory.Other, false, null);

        bill.ChangeCategory(BillCategory.Health);

        Assert.Equal(BillCategory.Health, bill.Category);
        Assert.Throws<DomainException>(() => bill.ChangeCategory(" "));
    }

    [Fact]
    public void Totals_MatchTheKarunarathnaBill()
    {
        var c = Content("si", "Karunarathna Stores", null,
            new BillItem("Papadam", 1, 210, markedPrice: 270), new BillItem("Flour", 1, 183, markedPrice: 183),
            new BillItem("Anchor", 1, 1260, markedPrice: 1300), new BillItem("Potatoes", 1.12m, 205, "kg", markedPrice: 205),
            new BillItem("Diabetasol", 3, 610, markedPrice: 675), new BillItem("Dhal", 1, 417, markedPrice: 417));
        Assert.Equal(4129.60m, c.Total);
        Assert.Equal(295m, c.Saved);
    }

    [Fact]
    public void Suggest_MatchesSinhalaKeywords()
    {
        var s = BillCategoriser.Suggest(Content("si", "Rasa", "රස මුළුතැන්ගෙය", new BillItem("Kottu", 1, 1350, nameSi: "කුකුළු මස් කොත්තු"), new BillItem("Rice", 2, 650, nameSi: "බත් සහ ව්‍යංජන")), null);
        Assert.Equal(BillCategory.Dining, s.Category);
        Assert.Equal(90, s.Confidence);
    }

    [Fact]
    public void Suggest_LearnedShopWins()
    {
        var s = BillCategoriser.Suggest(Content("en", "Green Leaf Supermarket", null, new BillItem("Rice", 1, 10)), BillCategory.Other);
        Assert.Equal((BillCategory.Other, 99), (s.Category, s.Confidence));
    }

    [Fact]
    public void Suggest_NoMatchIsOther()
    {
        Assert.Equal(BillCategory.Other, BillCategoriser.Suggest(Content("en", "Zzz", null, new BillItem("Qqq", 1, 10)), null).Category);
    }

    [Fact]
    public void Verdict_FlagsAboveMarket()
    {
        var v = PriceVerdict.For(new BillItem("Potatoes", 1.12m, 205, "kg"), new MarketPrice("p", 189, "kg", "Pettah", "potato"), 5);
        Assert.Equal(("8% above market", PriceVerdict.Bad), (v!.Text, v.Tone));
    }

    [Fact]
    public void Verdict_UsesMarkedPriceWithoutMarket()
    {
        var v = PriceVerdict.For(new BillItem("Papadam", 1, 210, markedPrice: 270), null, 5);
        Assert.Equal(("22% under marked price", PriceVerdict.Good), (v!.Text, v.Tone));
        Assert.Null(PriceVerdict.For(new BillItem("Flour", 1, 183, markedPrice: 183), null, 5));
    }

    [Fact]
    public void Create_RejectsMissingDate()
    {
        var c = Content("en", "Shop", null, new BillItem("A", 1, 1));
        Assert.Throws<DomainException>(() => Bill.Create(c, "Shop", null, 1, BillCategory.Other, true, null));
    }

    [Fact]
    public void Period_LastSevenDaysIncludesToday()
    {
        var week = BillPeriod.ForToday(new DateOnly(2026, 11, 3)).Single(p => p.Key == "week");
        Assert.True(week.Contains(new DateOnly(2026, 10, 28)));
        Assert.False(week.Contains(new DateOnly(2026, 10, 27)));
    }

    [Fact]
    public void FileType_RejectsHtmlPretendingToBeImage()
    {
        Assert.Null(BillFileType.Detect("<html>"u8));
        Assert.Equal(BillFileType.Pdf, BillFileType.Detect("%PDF-1.7"u8));
    }
}
