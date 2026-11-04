using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ExpenseTracker.Application;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.PriceWatch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Reading;

/// <summary>Adapter that reads bills through the signed-in Claude Code CLI on this machine.</summary>
public sealed class ClaudeCliBillReaderAdapter(string cliPath, IOptions<ClaudeOptions> options, ILogger<ClaudeCliBillReaderAdapter> logger) : IBillReader
{
    /// <summary>Finds the CLI executable from settings, then PATH; null when it is not installed.</summary>
    public static string? FindCli(string? configuredPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath)) return File.Exists(configuredPath) ? configuredPath : null;
        var folders = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        return folders
            .SelectMany(f => new[] { Path.Combine(f, OperatingSystem.IsWindows() ? "claude.exe" : "claude"), Path.Combine(f, "node_modules", "@anthropic-ai", "claude-code", "bin", "claude.exe") })
            .FirstOrDefault(File.Exists);
    }

    /// <inheritdoc />
    public async Task<BillReading> ReadAsync(byte[] data, string contentType, string? langHint, IReadOnlyList<MarketPrice> marketPrices, CancellationToken ct)
    {
        var folder = Directory.CreateTempSubdirectory("expense-bill-");
        try
        {
            var fileName = "bill" + Extension(contentType);
            await File.WriteAllBytesAsync(Path.Combine(folder.FullName, fileName), data, ct);
            var prompt = $"The bill is the file {fileName} in the current directory. Open it with the Read tool.\n\n{BillExtractionPrompt.Instruction(langHint, marketPrices)}";
            return ParseOutput(await RunAsync(folder.FullName, prompt, ct), marketPrices);
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    private async Task<string> RunAsync(string workingFolder, string prompt, CancellationToken ct)
    {
        var start = new ProcessStartInfo(cliPath)
        {
            WorkingDirectory = workingFolder,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in new[]
        {
            "-p", "--output-format", "json",
            "--json-schema", JsonSerializer.Serialize(BillExtractionPrompt.Schema()),
            "--system-prompt", BillExtractionPrompt.System,
            "--model", options.Value.Model, "--effort", "high",
            "--tools", "Read", "--allowedTools", "Read",
            "--restricted", "--strict-mcp-config", "--no-session-persistence",
        }) start.ArgumentList.Add(arg);
        // An inherited API key would bill the API instead of the signed-in account.
        start.Environment.Remove("ANTHROPIC_API_KEY");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.CliTimeoutSeconds));
        Process process;
        try
        {
            process = Process.Start(start) ?? throw new BillReadFailedException("Claude Code could not be started.");
        }
        catch (Win32Exception ex)
        {
            throw new BillReadFailedException("Claude Code could not be started. Check that it is installed.", ex);
        }

        using (process)
        {
            try
            {
                var output = process.StandardOutput.ReadToEndAsync(timeout.Token);
                var errors = process.StandardError.ReadToEndAsync(timeout.Token);
                await process.StandardInput.WriteAsync(prompt.AsMemory(), timeout.Token);
                process.StandardInput.Close();
                await process.WaitForExitAsync(timeout.Token);
                var text = await output;
                if (string.IsNullOrWhiteSpace(text))
                {
                    logger.LogError("Claude Code exited with {ExitCode}: {Errors}", process.ExitCode, await errors);
                    throw new BillReadFailedException("Claude Code could not read this bill. Check that it is signed in.");
                }
                return text;
            }
            catch (OperationCanceledException ex)
            {
                process.Kill(entireProcessTree: true);
                if (ct.IsCancellationRequested) throw;
                throw new BillReadFailedException("Reading the bill took too long. Try again.", ex);
            }
        }
    }

    private BillReading ParseOutput(string output, IReadOnlyList<MarketPrice> marketPrices)
    {
        try
        {
            using var document = JsonDocument.Parse(output);
            var root = document.RootElement;
            var failed = root.TryGetProperty("is_error", out var isError) && isError.GetBoolean();
            if (failed || !root.TryGetProperty("structured_output", out var structured) || structured.ValueKind != JsonValueKind.Object)
            {
                logger.LogError("Claude Code returned no bill: {Result}", root.TryGetProperty("result", out var result) ? result.ToString() : "(no result)");
                throw new BillReadFailedException("Claude Code could not read this bill. You may have reached your plan's usage limit, or the file may not be a bill.");
            }
            return BillExtractionParser.Parse(structured.GetRawText(), marketPrices);
        }
        catch (JsonException ex)
        {
            throw new BillReadFailedException("The bill reader returned something unexpected. Try again.", ex);
        }
    }

    private static string Extension(string contentType) => contentType switch
    {
        BillFileType.Pdf => ".pdf",
        BillFileType.Png => ".png",
        BillFileType.Webp => ".webp",
        _ => ".jpg",
    };
}
