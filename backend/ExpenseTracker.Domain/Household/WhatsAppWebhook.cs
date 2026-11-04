using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ExpenseTracker.Domain.Household;

/// <summary>One message delivered by WhatsApp.</summary>
public sealed record WhatsAppInbound(string MessageId, string From, string Type, string? MediaId, string? MimeType);

/// <summary>A delivered message already handled, so WhatsApp's retries are ignored.</summary>
public sealed class WhatsAppMessage
{
    private WhatsAppMessage() { Id = string.Empty; }

    /// <summary>Records a message id.</summary>
    public WhatsAppMessage(string id) => Id = id;

    public string Id { get; private set; }
}

/// <summary>The WhatsApp settings entered in the app; the single row holds protected secrets.</summary>
public sealed class WhatsAppConfig
{
    public const int SingleId = 1;

    public int Id { get; private set; } = SingleId;
    public string? VerifyToken { get; set; }
    public string? AppSecret { get; set; }
    public string? AccessToken { get; set; }
    public string? PhoneNumberId { get; set; }
}

/// <summary>Reads and checks what WhatsApp sends to the webhook.</summary>
public static class WhatsAppWebhook
{
    private const string SignaturePrefix = "sha256=";
    private const int LocalNumberLength = 10;
    private const string CountryCode = "94";

    /// <summary>True when the signature header was made from this body with the app secret.</summary>
    public static bool IsSigned(byte[] body, string? signature, string appSecret)
    {
        if (string.IsNullOrEmpty(appSecret) || signature is null || !signature.StartsWith(SignaturePrefix, StringComparison.Ordinal)) return false;
        var expected = Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(appSecret), body));
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature[SignaturePrefix.Length..]));
    }

    /// <summary>The number as WhatsApp reports it: digits only, with 0771234567 taken as a Sri Lankan number.</summary>
    public static string NormaliseNumber(string? number)
    {
        var digits = new string((number ?? "").Where(char.IsAsciiDigit).ToArray());
        return digits.Length == LocalNumberLength && digits[0] == '0' ? CountryCode + digits[1..] : digits;
    }

    /// <summary>Lists the messages in a delivery; status updates contain none.</summary>
    public static IReadOnlyList<WhatsAppInbound> Parse(byte[] body)
    {
        using var doc = JsonDocument.Parse(body);
        var found = new List<WhatsAppInbound>();
        foreach (var entry in Items(doc.RootElement, "entry"))
        foreach (var change in Items(entry, "changes"))
        {
            if (!change.TryGetProperty("value", out var value)) continue;
            foreach (var m in Items(value, "messages"))
            {
                var type = Text(m, "type") ?? "";
                var media = m.TryGetProperty(type, out var part) && part.ValueKind == JsonValueKind.Object ? part : default;
                var hasMedia = media.ValueKind == JsonValueKind.Object;
                if (Text(m, "id") is { } id && Text(m, "from") is { } from)
                    found.Add(new WhatsAppInbound(id, from, type, hasMedia ? Text(media, "id") : null, hasMedia ? Text(media, "mime_type") : null));
            }
        }

        return found;
    }

    private static IEnumerable<JsonElement> Items(JsonElement parent, string name) =>
        parent.TryGetProperty(name, out var list) && list.ValueKind == JsonValueKind.Array ? list.EnumerateArray().ToList() : [];

    private static string? Text(JsonElement parent, string name) =>
        parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}
