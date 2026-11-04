using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Tests;

public class ItemMappingTests
{
    private static readonly BillItem[] Eggs = [new("Eggs", 2, 50), new("Eggs 10 pack", 1, 400)];

    [Fact]
    public void Asks_about_similar_names_and_does_not_merge_them_unasked()
    {
        var question = Assert.Single(ItemTotals.FindQuestions(Eggs));

        Assert.Equal(["Eggs", "Eggs 10 pack"], new[] { question.NameA, question.NameB });
        Assert.Equal(2, ItemTotals.Summarise(Eggs).Count);
    }

    [Fact]
    public void Does_not_ask_about_unrelated_names()
    {
        BillItem[] items = [new("Doc. Fee", 1, 2700), new("Hos. Fee", 1, 1000), new("Anchor 400 g", 1, 1260), new("Anchor 1 kg", 1, 2900)];

        Assert.Empty(ItemTotals.FindQuestions(items));
    }

    [Fact]
    public void Does_not_ask_across_units()
    {
        Assert.Empty(ItemTotals.FindQuestions([new BillItem("Rice", 2, 300, "kg"), new BillItem("Rice 5", 1, 1500)]));
    }

    [Fact]
    public void Yes_merges_quantity_and_amount_and_stops_asking()
    {
        ItemMapping[] mappings = [new("eggs", "eggs 10 pack", true)];

        var total = Assert.Single(ItemTotals.Summarise(Eggs, mappings));

        Assert.Equal(3, total.Qty);
        Assert.Equal(500, total.Amount);
        Assert.Equal("Eggs 10 pack", total.Name);
        Assert.Empty(ItemTotals.FindQuestions(Eggs, mappings));
    }

    [Fact]
    public void No_keeps_items_apart_and_stops_asking()
    {
        ItemMapping[] mappings = [new("eggs 10 pack", "eggs", false)];

        Assert.Equal(2, ItemTotals.Summarise(Eggs, mappings).Count);
        Assert.Empty(ItemTotals.FindQuestions(Eggs, mappings));
    }
}
