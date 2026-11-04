using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Domain.Tests;

public class ItemTotalsTests
{
    [Fact]
    public void Combines_same_item_ignoring_case_and_sorts_by_spend()
    {
        var totals = ItemTotals.Summarise([new BillItem("Eggs", 2, 50), new BillItem("Rice", 1, 300), new BillItem("eggs", 1, 60)]);

        Assert.Equal(["Rice", "Eggs"], totals.Select(t => t.Name));
        Assert.Equal(3, totals[1].Qty);
        Assert.Equal(160, totals[1].Amount);
    }

    [Fact]
    public void Matches_on_sinhala_name_when_translations_differ()
    {
        var totals = ItemTotals.Summarise([new BillItem("Red dhal", 1, 300, null, "පරිප්පු"), new BillItem("Dhal", 1, 310, null, "පරිප්පු")]);

        Assert.Equal(610, Assert.Single(totals).Amount);
    }

    [Fact]
    public void Keeps_different_units_apart()
    {
        var totals = ItemTotals.Summarise([new BillItem("Rice", 2, 300, "kg"), new BillItem("Rice", 1, 700)]);

        Assert.Equal(2, totals.Count);
    }
}
