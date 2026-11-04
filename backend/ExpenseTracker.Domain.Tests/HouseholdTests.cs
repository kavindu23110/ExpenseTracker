using ExpenseTracker.Domain.Categorisation;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.Household;

namespace ExpenseTracker.Domain.Tests;

public class HouseholdTests
{
    [Fact]
    public void ChangeAmount_SetsQuantityAndTrimmedUnit()
    {
        var line = new ShoppingItem("rice", "Rice");
        line.ChangeAmount(2.5m, " kg ");
        Assert.Equal(2.5m, line.Qty);
        Assert.Equal("kg", line.Unit);
    }

    [Fact]
    public void ChangeAmount_BlankQuantityClearsTheAmount()
    {
        var line = new ShoppingItem("rice", "Rice");
        line.ChangeAmount(2, "kg");
        line.ChangeAmount(null, "kg");
        Assert.Null(line.Qty);
        Assert.Null(line.Unit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeAmount_RefusesZeroOrLess(int qty)
    {
        Assert.Throws<ExpenseTracker.Domain.DomainException>(() => new ShoppingItem("rice", "Rice").ChangeAmount(qty, "kg"));
    }

    private static readonly DateOnly Today = new(2026, 11, 3);

    private static Bill BillOn(string date, string shop, params BillItem[] items) =>
        Bill.Create(new BillContent("en", shop, null, null, null, null, DateOnly.Parse(date), null, null, null, null, items, []), shop, DateOnly.Parse(date), items.Sum(i => i.LineAmount), BillCategory.Groceries, true, null);

    [Fact]
    public void Budget_rejects_a_zero_limit() => Assert.Throws<DomainException>(() => new Budget(BillCategory.Dining, 0));

    [Fact]
    public void Goal_spreads_what_is_left_over_the_remaining_months()
    {
        var goal = new SavingsGoal("School fees", 12000, new DateOnly(2027, 1, 15));
        goal.AddSaving(3000);

        Assert.Equal(3000, goal.MonthlyNeeded(Today));
        Assert.Throws<DomainException>(() => goal.AddSaving(-4000));
    }

    [Fact]
    public void Goal_past_its_date_asks_for_the_rest_now() =>
        Assert.Equal(500, new SavingsGoal("Insurance", 500, new DateOnly(2026, 1, 1)).MonthlyNeeded(Today));

    [Theory]
    [InlineData(1, RecurringBill.Overdue)]
    [InlineData(5, RecurringBill.DueSoon)]
    [InlineData(20, RecurringBill.Upcoming)]
    public void Recurring_bill_status_follows_the_due_day(int day, string status) =>
        Assert.Equal(status, new RecurringBill("Water", 100, day, BillCategory.Utilities).Status(Today, 3));

    [Fact]
    public void Recurring_bill_is_paid_once_a_month_and_returns_next_month()
    {
        var bill = new RecurringBill("Rent", 100, 31, BillCategory.Other);
        bill.MarkPaid(Today);

        Assert.Equal(RecurringBill.Paid, bill.Status(Today, 3));
        Assert.Throws<DomainException>(() => bill.MarkPaid(Today));
        Assert.Equal(new DateOnly(2026, 11, 30), bill.DueDate(Today));
        Assert.Equal(RecurringBill.Upcoming, bill.Status(new DateOnly(2026, 12, 1), 3));
    }

    [Fact]
    public void Sms_reads_a_card_purchase_and_ignores_the_balance()
    {
        var p = Assert.Single(BankSmsParser.Parse("Purchase of LKR 1,250.50 at KEELLS SUPER on 03/11/2026 using card ending 1234. Avl Bal LKR 50,000.00"));

        Assert.Equal(1250.50m, p.Amount);
        Assert.Equal("KEELLS SUPER", p.Merchant);
        Assert.Equal(new DateOnly(2026, 11, 3), p.Date);
    }

    [Theory]
    [InlineData("Your OTP for the transaction of LKR 500.00 is 123456")]
    [InlineData("LKR 5,000.00 credited to your account")]
    [InlineData("Transaction of LKR 900.00 at ABC was declined")]
    [InlineData("Hello, your statement is ready")]
    public void Sms_skips_messages_that_are_not_payments(string message) => Assert.Empty(BankSmsParser.Parse(message));

    [Fact]
    public void Sms_keeps_unknown_shops_and_gives_the_same_message_the_same_reference()
    {
        const string message = "Rs. 300 debited from your account";
        var payments = BankSmsParser.Parse($"{message}\n{message}");

        Assert.Null(payments[0].Merchant);
        Assert.Equal(payments[0].Reference, payments[1].Reference);
    }

    [Fact]
    public void Price_history_needs_two_purchases_and_finds_the_rise_and_best_shop()
    {
        var history = PriceHistory.Build([
            BillOn("2026-10-01", "Shop A", new BillItem("Rice", 1, 200), new BillItem("Soap", 1, 50)),
            BillOn("2026-11-01", "Shop B", new BillItem("rice", 2, 250)),
        ]);

        var rice = Assert.Single(history);
        Assert.Equal(25, rice.ChangePercent);
        Assert.Equal("Shop A", rice.Shops[0].Shop);
        Assert.Equal(200, rice.Shops[0].Price);
    }

    [Fact]
    public void Price_history_ranks_shops_by_their_latest_price()
    {
        var history = PriceHistory.Build([
            BillOn("2026-09-01", "Shop B", new BillItem("Rice", 1, 190)),
            BillOn("2026-10-01", "shop b ", new BillItem("Rice", 1, 240)),
            BillOn("2026-10-15", "Shop A", new BillItem("Rice", 1, 210)),
            BillOn("2026-11-01", "Shop C", new BillItem("Rice", 1, 300)),
        ]);

        var shops = Assert.Single(history).Shops;
        Assert.Equal(["Shop A", "shop b", "Shop C"], shops.Select(s => s.Shop));
        Assert.Equal([210m, 240m, 300m], shops.Select(s => s.Price));
        Assert.Equal(new DateOnly(2026, 10, 1), shops[1].Date);
    }

    [Fact]
    public void Same_shop_date_and_total_is_a_duplicate()
    {
        var bill = BillOn("2026-11-03", "Shop A", new BillItem("Rice", 1, 200));

        Assert.True(bill.IsDuplicateOf(BillOn("2026-11-03", "shop a", new BillItem("Rice", 1, 200))));
        Assert.False(bill.IsDuplicateOf(BillOn("2026-11-04", "Shop A", new BillItem("Rice", 1, 200))));
        Assert.False(bill.IsDuplicateOf(BillOn("2026-11-03", "Shop A", new BillItem("Rice", 2, 200))));
    }

    [Theory]
    [InlineData("  ")]
    [InlineData(null)]
    public void Custom_category_needs_a_name(string? name) => Assert.Throws<DomainException>(() => new CustomCategory(name));

    [Fact]
    public void Custom_category_name_is_trimmed() => Assert.Equal("School fees", new CustomCategory(" School fees ").Name);

    [Fact]
    public void Credit_card_statement_is_suggested_as_credit_card()
    {
        var content = new BillContent("en", "Bank", null, null, null, null, new DateOnly(2026, 11, 1), null, null, null, null, [new BillItem("Credit card statement minimum payment", 1, 5000)], []);

        Assert.Equal(BillCategory.CreditCard, BillCategoriser.Suggest(content, null).Category);
    }

    [Fact]
    public void To_buy_suggests_last_months_groceries_still_needed()
    {
        var medical = Bill.Create(new BillContent("en", "Clinic", null, null, null, null, new DateOnly(2026, 10, 2), null, null, null, null, [new BillItem("Doctor fee", 1, 2000)], []), "Clinic", new DateOnly(2026, 10, 2), 2000, BillCategory.Health, true, null);
        var bills = new[]
        {
            medical,
            BillOn("2026-09-20", "Shop", new BillItem("Old item", 1, 10)),
            BillOn("2026-10-10", "Shop", new BillItem("Rice", 1, 200), new BillItem("Rice", 2, 200), new BillItem("Milk", 1, 300), new BillItem("Soap", 1, 90), new BillItem("Tea", 1, 80)),
            BillOn("2026-11-01", "Shop", new BillItem("milk", 1, 300)),
        };

        var suggested = ToBuySuggestions.Build(bills, [ItemTotals.KeyOf("Soap")], [], Today);

        Assert.Equal(["Rice", "Tea"], suggested.Select(s => s.Name));
        Assert.Equal(3, suggested[0].Qty);
    }

    [Fact]
    public void Known_items_are_past_groceries_not_on_the_list()
    {
        var bills = new[] { BillOn("2025-01-10", "Shop", new BillItem("Rice", 1, 200), new BillItem("Soap", 1, 90)) };

        Assert.Equal(["Rice"], ToBuySuggestions.Known(bills, [ItemTotals.KeyOf("Soap")], []).Select(s => s.Name));
    }

    [Fact]
    public void Amount_to_buy_is_the_quantity_as_written_on_the_bill()
    {
        var bills = new[]
        {
            BillOn("2026-09-05", "Shop", new BillItem("Rice", 9, 200, "kg"), new BillItem("Tea", 500, 1, "g")),
            BillOn("2026-10-05", "Shop", new BillItem("Rice", 5, 200, "kg")),
            BillOn("2026-10-09", "Shop", new BillItem("rice", 500, 0.2m, "g")),
            BillOn("2026-11-01", "Shop", new BillItem("Rice", 7, 200, "kg")),
        };

        var rice = ToBuySuggestions.AmountToBuy(bills, "RICE", Today);
        Assert.Equal(500, rice!.Qty);
        Assert.Equal("g", rice.Unit);
        Assert.Equal(500, ToBuySuggestions.AmountToBuy(bills, "Tea", Today)!.Qty);
        Assert.Null(ToBuySuggestions.AmountToBuy(bills, "Soap", Today));
    }

    [Fact]
    public void Last_month_total_counts_only_last_months_groceries()
    {
        var bills = new[]
        {
            BillOn("2026-09-30", "Shop", new BillItem("Tea", 1, 5)),
            BillOn("2026-10-01", "Shop", new BillItem("Rice", 1, 200)),
            BillOn("2026-10-31", "Shop", new BillItem("Milk", 1, 300)),
            BillOn("2026-11-01", "Shop", new BillItem("Soap", 1, 7)),
        };

        Assert.Equal(500, ToBuySuggestions.LastMonthTotal(bills, Today));
    }

    [Fact]
    public void Pantry_stops_at_zero()
    {
        var item = new PantryItem("rice", "Rice", 1, null);
        item.Adjust(-5);

        Assert.True(item.IsOut);
    }
}
