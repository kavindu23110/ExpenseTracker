namespace ExpenseTracker.Application.Ports;

/// <summary>Encrypts secrets kept in the database.</summary>
public interface ISecretProtector
{
    /// <summary>Encrypts a secret.</summary>
    string Protect(string secret);

    /// <summary>Decrypts a secret; null when it can no longer be read.</summary>
    string? Unprotect(string stored);
}

/// <summary>Talks to the WhatsApp Business service.</summary>
public interface IWhatsAppClient
{
    /// <summary>Asks the service for the business number's display number, proving the credentials work.</summary>
    Task<string> GetDisplayNumberAsync(CancellationToken ct);

    /// <summary>Downloads a file sent in a message.</summary>
    Task<byte[]> DownloadMediaAsync(string mediaId, CancellationToken ct);

    /// <summary>Sends a text reply.</summary>
    Task SendTextAsync(string to, string text, CancellationToken ct);
}
