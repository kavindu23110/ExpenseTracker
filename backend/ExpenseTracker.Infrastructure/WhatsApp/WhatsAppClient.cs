using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Application.Services;
using Microsoft.AspNetCore.DataProtection;

namespace ExpenseTracker.Infrastructure.WhatsApp;

/// <inheritdoc />
public sealed class SecretProtector(IDataProtectionProvider provider) : ISecretProtector
{
    private readonly IDataProtector protector = provider.CreateProtector("WhatsAppSettings");

    /// <inheritdoc />
    public string Protect(string secret) => protector.Protect(secret);

    /// <inheritdoc />
    public string? Unprotect(string stored)
    {
        try
        {
            return protector.Unprotect(stored);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}

/// <inheritdoc />
public sealed class WhatsAppClient(WhatsAppConfigService config) : IWhatsAppClient
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(2) };

    private sealed record MediaInfo([property: JsonPropertyName("url")] string Url);

    private sealed record NumberInfo([property: JsonPropertyName("display_phone_number")] string? DisplayPhoneNumber);

    /// <inheritdoc />
    public async Task<string> GetDisplayNumberAsync(CancellationToken ct)
    {
        var o = await config.GetAsync(ct);
        using var lookup = Request(o, HttpMethod.Get, $"{o.GraphBaseUrl}/{Uri.EscapeDataString(o.PhoneNumberId)}?fields=display_phone_number");
        using var found = await Http.SendAsync(lookup, ct);
        found.EnsureSuccessStatusCode();
        return (await found.Content.ReadFromJsonAsync<NumberInfo>(ct))?.DisplayPhoneNumber ?? "the business number";
    }

    /// <inheritdoc />
    public async Task<byte[]> DownloadMediaAsync(string mediaId, CancellationToken ct)
    {
        var o = await config.GetAsync(ct);
        using var lookup = Request(o, HttpMethod.Get, $"{o.GraphBaseUrl}/{Uri.EscapeDataString(mediaId)}");
        using var found = await Http.SendAsync(lookup, ct);
        found.EnsureSuccessStatusCode();
        var media = await found.Content.ReadFromJsonAsync<MediaInfo>(ct) ?? throw new InvalidOperationException("WhatsApp returned no media address.");
        using var download = Request(o, HttpMethod.Get, media.Url);
        using var file = await Http.SendAsync(download, ct);
        file.EnsureSuccessStatusCode();
        return await file.Content.ReadAsByteArrayAsync(ct);
    }

    /// <inheritdoc />
    public async Task SendTextAsync(string to, string text, CancellationToken ct)
    {
        var o = await config.GetAsync(ct);
        using var send = Request(o, HttpMethod.Post, $"{o.GraphBaseUrl}/{Uri.EscapeDataString(o.PhoneNumberId)}/messages");
        send.Content = JsonContent.Create(new Dictionary<string, object>
        {
            ["messaging_product"] = "whatsapp",
            ["to"] = to,
            ["type"] = "text",
            ["text"] = new Dictionary<string, string> { ["body"] = text },
        });
        using var sent = await Http.SendAsync(send, ct);
        sent.EnsureSuccessStatusCode();
    }

    private static HttpRequestMessage Request(WhatsAppOptions o, HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", o.AccessToken);
        return request;
    }
}
