using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Ports;
using ExpenseTracker.Domain.Bills;

namespace ExpenseTracker.Application.Services;

/// <summary>Price histories from the household's own bills.</summary>
public sealed class PriceHistoryService(IBillRepository bills, IItemMappingRepository mappings)
{
    /// <summary>Lists products bought at least twice.</summary>
    public async Task<IReadOnlyList<PriceHistoryDto>> GetAsync(CancellationToken ct)
    {
        var histories = PriceHistory.Build(await bills.GetAllAsync(ct), await mappings.GetAllAsync(ct));
        return histories.Select(h =>
        {
            var max = h.Points.Max(p => p.Price);
            var points = h.Points.Select(p => new PricePointDto(p.Date.ToString("yyyy-MM-dd"), p.Shop, p.Price, max == 0 ? 0 : (int)Math.Round(p.Price / max * 100))).ToList();
            return new PriceHistoryDto(h.Name, h.NameSi, h.Unit, points, h.ChangePercent, h.Shops.Select(s => new ShopPriceDto(s.Shop, s.Price, s.Date.ToString("yyyy-MM-dd"))).ToList());
        }).ToList();
    }
}
