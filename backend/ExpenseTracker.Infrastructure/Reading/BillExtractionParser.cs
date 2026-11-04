using System.Text.Json;
using ExpenseTracker.Application;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.PriceWatch;

namespace ExpenseTracker.Infrastructure.Reading;

/// <summary>Turns the reader's structured JSON into a validated bill reading.</summary>
internal static class BillExtractionParser
{
    /// <summary>Parses the JSON; failures become read errors the user can act on.</summary>
    public static BillReading Parse(string json, IReadOnlyList<MarketPrice> marketPrices)
    {
        try
        {
            var extraction = JsonSerializer.Deserialize<BillExtraction>(json) ?? throw new BillReadFailedException("The bill reader returned nothing.");
            var reading = extraction.ToReading(marketPrices.Select(m => m.Key).ToHashSet());
            if (reading.Content.Items.Count == 0) throw new BillReadFailedException("No line items were found. Is this a bill?");
            return reading;
        }
        catch (JsonException ex)
        {
            throw new BillReadFailedException("The bill reader returned something unexpected. Try again.", ex);
        }
        catch (DomainException ex)
        {
            throw new BillReadFailedException($"The bill could not be read clearly: {ex.Message}", ex);
        }
    }
}
