using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Household;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Application.Services;

/// <summary>Settings for the WhatsApp Business number.</summary>
public sealed class WhatsAppOptions
{
    public const string Section = "WhatsApp";

    public string VerifyToken { get; set; } = "";
    public string AppSecret { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string PhoneNumberId { get; set; } = "";
    public string GraphBaseUrl { get; set; } = "https://graph.facebook.com/v23.0";

    /// <summary>True when every setting needed to receive and answer messages is present.</summary>
    public bool IsConfigured => VerifyToken != "" && AppSecret != "" && AccessToken != "" && PhoneNumberId != "";
}

/// <summary>A change to the WhatsApp settings; blank values keep what is saved.</summary>
public sealed record SaveWhatsAppConfigRequest(string? VerifyToken, string? AppSecret, string? AccessToken, string? PhoneNumberId);

/// <summary>The WhatsApp settings in force: those saved in the app, otherwise those from the server's configuration.</summary>
public sealed class WhatsAppConfigService(IStore<WhatsAppConfig> store, ISecretProtector protector, IOptions<WhatsAppOptions> defaults)
{
    /// <summary>Loads the settings in force.</summary>
    public async Task<WhatsAppOptions> GetAsync(CancellationToken ct)
    {
        var saved = await store.FindAsync(WhatsAppConfig.SingleId, ct);
        var d = defaults.Value;
        return new WhatsAppOptions
        {
            VerifyToken = Secret(saved?.VerifyToken) ?? d.VerifyToken,
            AppSecret = Secret(saved?.AppSecret) ?? d.AppSecret,
            AccessToken = Secret(saved?.AccessToken) ?? d.AccessToken,
            PhoneNumberId = saved?.PhoneNumberId ?? d.PhoneNumberId,
            GraphBaseUrl = d.GraphBaseUrl,
        };
    }

    /// <summary>Saves the values given, encrypting the secrets.</summary>
    public async Task SaveAsync(SaveWhatsAppConfigRequest r, CancellationToken ct)
    {
        var saved = await store.FindAsync(WhatsAppConfig.SingleId, ct);
        if (saved is null)
        {
            saved = new WhatsAppConfig();
            store.Add(saved);
        }

        if (!string.IsNullOrWhiteSpace(r.VerifyToken)) saved.VerifyToken = protector.Protect(r.VerifyToken.Trim());
        if (!string.IsNullOrWhiteSpace(r.AppSecret)) saved.AppSecret = protector.Protect(r.AppSecret.Trim());
        if (!string.IsNullOrWhiteSpace(r.AccessToken)) saved.AccessToken = protector.Protect(r.AccessToken.Trim());
        if (!string.IsNullOrWhiteSpace(r.PhoneNumberId)) saved.PhoneNumberId = r.PhoneNumberId.Trim();
        await store.SaveAsync(ct);
    }

    /// <summary>Removes the settings saved in the app.</summary>
    public async Task ClearAsync(CancellationToken ct)
    {
        if (await store.FindAsync(WhatsAppConfig.SingleId, ct) is not { } saved) return;
        store.Remove(saved);
        await store.SaveAsync(ct);
    }

    private string? Secret(string? stored) => stored is null ? null : protector.Unprotect(stored);
}

/// <summary>Remembers when WhatsApp last reached this app, for the settings page.</summary>
public sealed class WhatsAppActivity
{
    /// <summary>When a signed delivery last arrived; null since the app started.</summary>
    public DateTimeOffset? LastReceived { get; set; }
}

/// <summary>Whether a WhatsApp setting is present; never its value.</summary>
public sealed record WhatsAppSettingDto(string Name, bool Present);

/// <summary>The WhatsApp section of the settings page.</summary>
public sealed record WhatsAppStatusDto(bool Configured, IReadOnlyList<WhatsAppSettingDto> Settings, int LinkedMembers, DateTimeOffset? LastReceived);

/// <summary>The outcome of a WhatsApp test.</summary>
public sealed record WhatsAppTestResult(bool Ok, string Message);

/// <summary>A number to send a test message to.</summary>
public sealed record WhatsAppTestRequest(string? Number);

/// <summary>Checks the WhatsApp setup for the owner.</summary>
public sealed class WhatsAppSettingsService(WhatsAppConfigService config, IStore<AppUser> users, IWhatsAppClient whatsApp, WhatsAppActivity activity)
{
    /// <summary>Reports which settings are present.</summary>
    public async Task<WhatsAppStatusDto> GetStatusAsync(CancellationToken ct)
    {
        var o = await config.GetAsync(ct);
        return new(o.IsConfigured, [
            new("Verify token", o.VerifyToken != ""),
            new("App secret", o.AppSecret != ""),
            new("Access token", o.AccessToken != ""),
            new("Phone number ID", o.PhoneNumberId != ""),
        ], (await users.ListAsync(ct)).Count(u => u.WhatsAppNumber is not null), activity.LastReceived);
    }

    /// <summary>Tries the credentials against the service.</summary>
    public Task<WhatsAppTestResult> CheckConnectionAsync(CancellationToken ct) => TryAsync(async () => $"Connected to {await whatsApp.GetDisplayNumberAsync(ct)}.", "Meta refused the connection. Check the access token and phone number ID.");

    /// <summary>Sends a test message.</summary>
    public Task<WhatsAppTestResult> SendTestAsync(string? number, CancellationToken ct)
    {
        var to = WhatsAppWebhook.NormaliseNumber(number);
        if (to.Length < 8) return Task.FromResult(new WhatsAppTestResult(false, "Enter the number with its country code."));
        return TryAsync(async () =>
        {
            await whatsApp.SendTextAsync(to, "Expense Tracker test message: this number is working.", ct);
            return $"Meta accepted the message to {to}. Check that it arrives on the phone.";
        }, "Meta refused the message. Check the settings, and that this number messaged the business number in the last 24 hours.");
    }

    private async Task<WhatsAppTestResult> TryAsync(Func<Task<string>> test, string failure)
    {
        if (!(await config.GetAsync(CancellationToken.None)).IsConfigured) return new(false, "Add the missing settings first.");
        try
        {
            return new(true, await test());
        }
        catch (HttpRequestException)
        {
            return new(false, failure);
        }
    }
}

/// <summary>Reads bills sent by household members to the WhatsApp number and saves them straight away.</summary>
public sealed class WhatsAppBillService(IStore<AppUser> users, IStore<WhatsAppMessage> handled, IWhatsAppClient whatsApp, IBillScanService scanner, IBillFilingService filing)
{
    private static readonly Dictionary<string, string> Extensions = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["application/pdf"] = ".pdf",
    };

    /// <summary>Handles one message once; numbers not linked to a member are ignored without a reply.</summary>
    public async Task ProcessAsync(WhatsAppInbound message, CancellationToken ct)
    {
        var sender = WhatsAppWebhook.NormaliseNumber(message.From);
        if (!(await users.ListAsync(ct)).Any(u => u.WhatsAppNumber == sender)) return;
        if (await handled.FindAsync(message.MessageId, ct) is not null) return;
        handled.Add(new WhatsAppMessage(message.MessageId));
        await handled.SaveAsync(ct);

        if (message.MediaId is null || message.MimeType is null || !Extensions.TryGetValue(message.MimeType.Split(';')[0].Trim(), out var extension))
        {
            await whatsApp.SendTextAsync(sender, "Send a bill as a photo or PDF and I'll save it.", ct);
            return;
        }

        string reply;
        try
        {
            var data = await whatsApp.DownloadMediaAsync(message.MediaId, ct);
            var draft = await scanner.ScanAsync(new UploadedFile($"whatsapp-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}", data), null, ct);
            var category = draft.Suggestion.Category;
            await filing.SaveAsync(new SaveBillRequest(
                draft.Lang, draft.Merchant, draft.MerchantSi, draft.Address, draft.AddressSi, draft.BillNo, draft.Date, draft.Time,
                draft.Payment, draft.PaymentSi, draft.Paid, draft.Amount, category, category, draft.FileName, draft.Items, draft.Charges), new UploadedFile(draft.FileName, data), ct);
            reply = $"Saved Rs {draft.Amount:N2} at {draft.Merchant} under {category}.";
            if (draft.TotalMismatch) reply += $" The items add up to Rs {draft.ComputedTotal:N2}, so please check this bill in the app.";
        }
        catch (DomainException e)
        {
            reply = $"Not saved. {e.Message}";
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            await whatsApp.SendTextAsync(sender, "That bill could not be read, so it wasn't saved. Please try a clearer photo.", ct);
            throw;
        }

        await whatsApp.SendTextAsync(sender, reply, ct);
    }
}
