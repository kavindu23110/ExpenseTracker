using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using ExpenseTracker.Application;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Bills;
using ExpenseTracker.Domain.PriceWatch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Reading;

/// <summary>Adapter that reads bills with Claude vision and structured output.</summary>
public sealed class ClaudeBillReaderAdapter(AnthropicClient client, IOptions<ClaudeOptions> options, ILogger<ClaudeBillReaderAdapter> logger) : IBillReader
{
    /// <inheritdoc />
    public async Task<BillReading> ReadAsync(byte[] data, string contentType, string? langHint, IReadOnlyList<MarketPrice> marketPrices, CancellationToken ct)
    {
        Message response;
        try
        {
            response = await client.Messages.Create(new MessageCreateParams
            {
                Model = options.Value.Model,
                MaxTokens = options.Value.MaxTokens,
                System = BillExtractionPrompt.System,
                OutputConfig = new OutputConfig { Effort = Effort.High, Format = new JsonOutputFormat { Schema = BillExtractionPrompt.Schema() } },
                Messages =
                [
                    new UserMessageParam
                    {
                        Content = new List<ContentBlockParam>
                        {
                            FileBlock(data, contentType),
                            new TextBlockParam { Text = BillExtractionPrompt.Instruction(langHint, marketPrices) },
                        },
                    },
                ],
            }, ct);
        }
        catch (AnthropicRateLimitException ex)
        {
            throw new BillReadFailedException("The bill reader is busy. Try again in a minute.", ex);
        }
        catch (Anthropic5xxException ex)
        {
            throw new BillReadFailedException("The bill reader is having problems. Try again shortly.", ex);
        }
        catch (AnthropicApiException ex)
        {
            logger.LogError(ex, "Claude rejected a bill read request");
            throw new BillReadFailedException("The bill could not be sent for reading. Check the API key and settings.", ex);
        }

        return Parse(response, marketPrices);
    }

    private static ContentBlockParam FileBlock(byte[] data, string contentType)
    {
        var base64 = Convert.ToBase64String(data);
        if (contentType == BillFileType.Pdf) return new DocumentBlockParam { Source = new Base64PdfSource { Data = base64 } };
        return new ImageBlockParam
        {
            Source = new Base64ImageSource
            {
                Data = base64,
                MediaType = contentType switch
                {
                    BillFileType.Png => MediaType.ImagePng,
                    BillFileType.Webp => MediaType.ImageWebP,
                    _ => MediaType.ImageJpeg,
                },
            },
        };
    }

    private static BillReading Parse(Message response, IReadOnlyList<MarketPrice> marketPrices)
    {
        if (response.StopReason == StopReason.Refusal) throw new BillReadFailedException("This file could not be read as a bill.");
        if (response.StopReason == StopReason.MaxTokens) throw new BillReadFailedException("This bill is too long to read in one go.");

        var json = string.Concat(response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));
        return BillExtractionParser.Parse(json, marketPrices);
    }
}
