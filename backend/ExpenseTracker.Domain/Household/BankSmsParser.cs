using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Domain.Household;

/// <summary>A card or bank payment found in a text message.</summary>
public sealed record SmsPayment(decimal Amount, string? Merchant, DateOnly? Date, string Reference);

/// <summary>Finds outgoing payments in pasted bank messages, one message per line.</summary>
public static partial class BankSmsParser
{
    private static readonly string[] DateFormats = ["dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy", "d/M/yy", "dd-MM-yyyy", "dd-MM-yy", "dd-MMM-yyyy", "dd-MMM-yy", "dd MMM yyyy", "yyyy-MM-dd"];

    /// <summary>Returns the payments; other messages such as OTPs and deposits are skipped.</summary>
    public static IReadOnlyList<SmsPayment> Parse(string? text)
    {
        var payments = new List<SmsPayment>();
        foreach (var line in (text ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (IgnoredMessage().IsMatch(line) || !PaymentMessage().IsMatch(line)) continue;
            var amount = AmountPattern().Match(line);
            if (!amount.Success || !decimal.TryParse(amount.Groups[1].Value.Replace(",", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out var value) || value <= 0) continue;
            var merchant = MerchantPattern().Match(line);
            payments.Add(new SmsPayment(value, merchant.Success ? merchant.Groups[1].Value.Trim() : null, FindDate(line), Reference(line)));
        }

        return payments;
    }

    private static DateOnly? FindDate(string line)
    {
        var match = DatePattern().Match(line);
        return match.Success && DateOnly.TryParseExact(match.Value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
    }

    private static string Reference(string line) => "SMS-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(line)))[..16];

    [GeneratedRegex(@"\b(otp|one[- ]time|verification code|credited|deposit(ed)?|received|refund(ed)?|reversed|reversal|declined|failed|unsuccessful)\b", RegexOptions.IgnoreCase)]
    private static partial Regex IgnoredMessage();

    [GeneratedRegex(@"\b(purchase|purchased|debited|debit|spent|withdrawn|withdrawal|paid|payment|pos|transaction|txn)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentMessage();

    [GeneratedRegex(@"(?:LKR|Rs\.?)\s*([\d,]+(?:\.\d{1,2})?)", RegexOptions.IgnoreCase)]
    private static partial Regex AmountPattern();

    [GeneratedRegex(@"\b(?:at|to|@)\s+([A-Za-z][A-Za-z0-9&'\-. ]{1,40}?)(?=\s+(?:on|from|using|via|by|with|card|ref|avl|avail|available|bal|balance)\b|[,;]|\.\s|\.$|\s+\d|$)", RegexOptions.IgnoreCase)]
    private static partial Regex MerchantPattern();

    [GeneratedRegex(@"\b(?:\d{4}-\d{2}-\d{2}|\d{1,2}[/-]\d{1,2}[/-]\d{2,4}|\d{2}[- ][A-Za-z]{3}[- ]\d{2,4})\b")]
    private static partial Regex DatePattern();
}
