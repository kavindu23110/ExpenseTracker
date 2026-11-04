using System.Threading.Channels;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Household;
using Microsoft.AspNetCore.Authorization;

namespace ExpenseTracker.Api;

/// <summary>The public address WhatsApp delivers messages to; it answers straight away and works through them in the background.</summary>
public static class WhatsAppEndpoints
{
    /// <summary>Maps the webhook.</summary>
    public static void MapWhatsAppEndpoints(this WebApplication app)
    {
        app.MapGet("/api/whatsapp/webhook", async (HttpRequest r, WhatsAppConfigService config, CancellationToken ct) =>
        {
            var o = await config.GetAsync(ct);
            return o.IsConfigured && r.Query["hub.mode"] == "subscribe" && r.Query["hub.verify_token"] == o.VerifyToken
                ? Results.Text(r.Query["hub.challenge"].ToString())
                : Results.StatusCode(StatusCodes.Status403Forbidden);
        });

        app.MapPost("/api/whatsapp/webhook", async (HttpRequest r, WhatsAppConfigService config, Channel<WhatsAppInbound> queue, WhatsAppActivity activity, CancellationToken ct) =>
        {
            var o = await config.GetAsync(ct);
            if (!o.IsConfigured) return Results.NotFound();
            using var buffer = new MemoryStream();
            await r.Body.CopyToAsync(buffer, ct);
            var body = buffer.ToArray();
            if (!WhatsAppWebhook.IsSigned(body, r.Headers["X-Hub-Signature-256"], o.AppSecret)) return Results.Unauthorized();
            activity.LastReceived = DateTimeOffset.UtcNow;
            foreach (var message in WhatsAppWebhook.Parse(body)) await queue.Writer.WriteAsync(message, ct);
            return Results.Ok();
        });

        var owner = new AuthorizeAttribute { Roles = AuthEndpoints.OwnerRole };
        app.MapGet("/api/settings/whatsapp", (WhatsAppSettingsService s, CancellationToken ct) => s.GetStatusAsync(ct)).RequireAuthorization(owner);
        app.MapPut("/api/settings/whatsapp", async (SaveWhatsAppConfigRequest r, WhatsAppConfigService s, CancellationToken ct) => { await s.SaveAsync(r, ct); return Results.NoContent(); }).RequireAuthorization(owner);
        app.MapDelete("/api/settings/whatsapp", async (WhatsAppConfigService s, CancellationToken ct) => { await s.ClearAsync(ct); return Results.NoContent(); }).RequireAuthorization(owner);
        app.MapPost("/api/settings/whatsapp/check", (WhatsAppSettingsService s, CancellationToken ct) => s.CheckConnectionAsync(ct)).RequireAuthorization(owner);
        app.MapPost("/api/settings/whatsapp/test-message", (WhatsAppTestRequest r, WhatsAppSettingsService s, CancellationToken ct) => s.SendTestAsync(r.Number, ct)).RequireAuthorization(owner);
    }
}

/// <summary>Handles delivered messages one at a time, because reading a bill can take minutes.</summary>
public sealed class WhatsAppWorker(Channel<WhatsAppInbound> queue, IServiceScopeFactory scopes, ILogger<WhatsAppWorker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<WhatsAppBillService>().ProcessAsync(message, stoppingToken);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "A WhatsApp message could not be handled.");
            }
        }
    }
}
