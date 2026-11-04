using System.Globalization;
using System.Text.Json.Serialization;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Infrastructure.Reading;

/// <summary>The structured output returned by the reader, mapped onto the domain.</summary>
internal sealed record BillExtraction(
    [property: JsonPropertyName("lang")] string Lang,
    [property: JsonPropertyName("merchant")] string? Merchant,
    [property: JsonPropertyName("merchant_si")] string? MerchantSi,
    [property: JsonPropertyName("address")] string? Address,
    [property: JsonPropertyName("address_si")] string? AddressSi,
    [property: JsonPropertyName("bill_no")] string? BillNo,
    [property: JsonPropertyName("date")] string? Date,
    [property: JsonPropertyName("time")] string? Time,
    [property: JsonPropertyName("payment")] string? Payment,
    [property: JsonPropertyName("payment_si")] string? PaymentSi,
    [property: JsonPropertyName("paid")] decimal? Paid,
    [property: JsonPropertyName("total")] decimal? Total,
    [property: JsonPropertyName("items")] List<ExtractedItem> Items,
    [property: JsonPropertyName("charges")] List<ExtractedCharge> Charges)
{
    /// <summary>Validates through the domain; market keys outside the known list are dropped.</summary>
    public BillReading ToReading(IReadOnlySet<string> marketKeys)
    {
        var items = Items.Select(i => new BillItem(i.Name, i.Qty, i.Price, i.Unit, i.NameSi, i.MarkedPrice, i.MarketKey is not null && marketKeys.Contains(i.MarketKey) ? i.MarketKey : null)).ToList();
        var charges = Charges.Select(c => new BillCharge(c.Label, c.Amount, c.LabelSi)).ToList();
        var date = DateOnly.TryParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : (DateOnly?)null;
        var time = TimeOnly.TryParse(Time, CultureInfo.InvariantCulture, out var t) ? t : (TimeOnly?)null;
        var lang = BillLanguage.IsValid(Lang) ? Lang : BillLanguage.English;
        return new BillReading(new BillContent(lang, Merchant ?? MerchantSi ?? string.Empty, MerchantSi, Address, AddressSi, BillNo, date, time, Payment, PaymentSi, Paid, items, charges), Total);
    }
}

/// <summary>An extracted line item.</summary>
internal sealed record ExtractedItem(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("name_si")] string? NameSi,
    [property: JsonPropertyName("qty")] decimal Qty,
    [property: JsonPropertyName("unit")] string? Unit,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("marked_price")] decimal? MarkedPrice,
    [property: JsonPropertyName("market_key")] string? MarketKey);

/// <summary>An extracted charge.</summary>
internal sealed record ExtractedCharge(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("label_si")] string? LabelSi,
    [property: JsonPropertyName("amount")] decimal Amount);
