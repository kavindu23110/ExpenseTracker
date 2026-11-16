using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ExpenseTracker.Application.Ports;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Superset;

/// <summary>Settings for the Superset dashboard on the Statistics page.</summary>
public sealed class SupersetOptions
{
    public const string Section = "Superset";

    /// <summary>Address the API uses, such as http://superset:8088.</summary>
    public string InternalUrl { get; set; } = "";

    /// <summary>Address the browser uses to load the dashboard.</summary>
    public string PublicUrl { get; set; } = "";

    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    /// <summary>The embed ID Superset shows under Dashboard → Embed dashboard.</summary>
    public string DashboardId { get; set; } = "";

    public bool IsConfigured => InternalUrl != "" && PublicUrl != "" && Username != "" && Password != "" && DashboardId != "";
}

/// <summary>Signs in to Superset as the service account and asks for guest tokens.</summary>
public sealed class SupersetClient(IOptions<SupersetOptions> options) : ISupersetClient
{
    /// <inheritdoc />
    public SupersetEmbedDto Embed => options.Value.IsConfigured
        ? new(true, options.Value.PublicUrl.TrimEnd('/'), options.Value.DashboardId)
        : new(false, null, null);

    /// <inheritdoc />
    public async Task<string> GetGuestTokenAsync(string memberName, CancellationToken ct)
    {
        var o = options.Value;
        if (!o.IsConfigured) throw new InvalidOperationException("Superset is not set up.");
        var baseUrl = o.InternalUrl.TrimEnd('/');
        // A fresh cookie jar per request keeps one member's Superset session separate from another's.
        using var handler = new HttpClientHandler { CookieContainer = new System.Net.CookieContainer() };
        using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

        var login = await http.PostAsJsonAsync($"{baseUrl}/api/v1/security/login", new { username = o.Username, password = o.Password, provider = "db", refresh = false }, ct);
        login.EnsureSuccessStatusCode();
        var access = (await login.Content.ReadFromJsonAsync<LoginResponse>(ct))!.AccessToken;

        // The guest token call is CSRF-protected; the token is tied to the session cookie the handler keeps.
        using var csrfRequest = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/v1/security/csrf_token/");
        csrfRequest.Headers.Authorization = new("Bearer", access);
        var csrf = await http.SendAsync(csrfRequest, ct);
        csrf.EnsureSuccessStatusCode();
        var csrfToken = (await csrf.Content.ReadFromJsonAsync<CsrfResponse>(ct))!.Result;

        using var guestRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/v1/security/guest_token/")
        {
            Content = JsonContent.Create(new
            {
                user = new { username = memberName, first_name = memberName, last_name = "" },
                resources = new[] { new { type = "dashboard", id = o.DashboardId } },
                rls = Array.Empty<object>(),
            }),
        };
        guestRequest.Headers.Authorization = new("Bearer", access);
        guestRequest.Headers.Add("X-CSRFToken", csrfToken);
        guestRequest.Headers.Referrer = new Uri(baseUrl + "/");
        var guest = await http.SendAsync(guestRequest, ct);
        guest.EnsureSuccessStatusCode();
        return (await guest.Content.ReadFromJsonAsync<GuestTokenResponse>(ct))!.Token;
    }

    private sealed record LoginResponse([property: JsonPropertyName("access_token")] string AccessToken);
    private sealed record CsrfResponse([property: JsonPropertyName("result")] string Result);
    private sealed record GuestTokenResponse([property: JsonPropertyName("token")] string Token);
}
