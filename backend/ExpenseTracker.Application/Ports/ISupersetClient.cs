namespace ExpenseTracker.Application.Ports;

/// <summary>Where the Superset dashboard is shown from, as the browser needs it.</summary>
public sealed record SupersetEmbedDto(bool Enabled, string? PublicUrl, string? DashboardId);

/// <summary>Talks to Apache Superset to let a signed-in member view the embedded dashboard.</summary>
public interface ISupersetClient
{
    /// <summary>The dashboard settings, or disabled when Superset is not set up.</summary>
    SupersetEmbedDto Embed { get; }

    /// <summary>Gets a short-lived token that lets this member view the dashboard and nothing else.</summary>
    Task<string> GetGuestTokenAsync(string memberName, CancellationToken ct);
}
