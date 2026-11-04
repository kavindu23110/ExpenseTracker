namespace ExpenseTracker.Infrastructure.Reading;

/// <summary>Settings for the Claude bill reader, bound from the Claude section.</summary>
public sealed class ClaudeOptions
{
    public const string Section = "Claude";

    public string? ApiKey { get; set; }
    public bool UseCli { get; set; }
    public string? CliPath { get; set; }
    public int CliTimeoutSeconds { get; set; } = 180;
    public string Model { get; set; } = "claude-opus-5-5";
    public int MaxTokens { get; set; } = 16000;
}
