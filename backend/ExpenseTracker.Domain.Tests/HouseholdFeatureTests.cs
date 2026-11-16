using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Household;

namespace ExpenseTracker.Domain.Tests;

public class HouseholdFeatureTests
{
    private static Bill BillOn(string date, string shop, decimal amount, string? paidBy = null)
    {
        var day = DateOnly.Parse(date);
        var content = new BillContent(BillLanguage.English, shop, null, null, null, null, day, null, null, null, null, [new BillItem("Thing", 1, amount)], []);
        var bill = Bill.Create(content, shop, day, amount, BillCategory.Other, false, null);
        bill.SetPaidBy(paidBy);
        return bill;
    }

    [Fact]
    public void Shared_bills_are_split_equally_and_settlements_count()
    {
        var bills = new[] { BillOn("2026-11-01", "Shop", 3000, "Amal"), BillOn("2026-11-02", "Shop", 600, "Nimal"), BillOn("2026-11-03", "Shop", 999) };
        var balances = HouseholdBalance.Compute(["Amal", "Nimal", "Sita"], bills, [new Settlement("Sita", "Amal", 200, new DateOnly(2026, 11, 4))]);

        Assert.Equal(1200, balances[0].Share);
        Assert.Equal(1600, balances.Single(b => b.Member == "Amal").Balance);
        Assert.Equal(-600, balances.Single(b => b.Member == "Nimal").Balance);
        Assert.Equal(-1000, balances.Single(b => b.Member == "Sita").Balance);

        var payments = HouseholdBalance.SettleUp(balances);
        Assert.Equal([new SuggestedPayment("Sita", "Amal", 1000), new SuggestedPayment("Nimal", "Amal", 600)], payments);
    }

    [Fact]
    public void A_similar_charge_each_month_is_a_subscription()
    {
        var today = new DateOnly(2026, 11, 16);
        var bills = new[]
        {
            BillOn("2026-08-05", "Netflix", 1500), BillOn("2026-09-05", "Netflix", 1500), BillOn("2026-10-06", "netflix ", 1550),
            BillOn("2026-09-01", "Keells", 4000), BillOn("2026-09-08", "Keells", 4100), BillOn("2026-10-01", "Keells", 3900), BillOn("2026-11-01", "Keells", 4050),
            BillOn("2026-08-10", "Dialog", 2000), BillOn("2026-09-10", "Dialog", 2000), BillOn("2026-10-10", "Dialog", 2000),
        };

        var found = SubscriptionFinder.Find(bills, ["Dialog"], today);

        var netflix = Assert.Single(found);
        Assert.Equal(1500, netflix.Amount);
        Assert.Equal(5, netflix.DayOfMonth);
        Assert.Equal(3, netflix.Months);
    }

    [Fact]
    public void Pantry_item_is_low_at_its_level_and_flags_expiry()
    {
        var item = new PantryItem("rice", "Rice", 3, "kg");
        item.SetDetails(new DateOnly(2026, 11, 18), " Fridge ", 2);

        Assert.False(item.IsLow);
        item.Adjust(-1);
        Assert.True(item.IsLow);
        Assert.Equal("Fridge", item.Location);
        Assert.True(item.ExpiresBy(new DateOnly(2026, 11, 19)));
        item.UseUp();
        Assert.False(item.IsLow);
        Assert.False(item.ExpiresBy(new DateOnly(2026, 11, 19)));
    }
}
