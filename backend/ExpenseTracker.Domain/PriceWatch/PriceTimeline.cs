namespace ExpenseTracker.Domain.PriceWatch;

/// <summary>A price history for one product, drawn as bars above a base price.</summary>
public sealed class PriceTimeline
{
    private readonly List<PriceTimelinePoint> _points = [];

    private PriceTimeline() { Title = string.Empty; Summary = string.Empty; Foot = string.Empty; }

    /// <summary>Creates a timeline with its points in order.</summary>
    public PriceTimeline(int sort, string title, decimal basePrice, string summary, string foot, IEnumerable<PriceTimelinePoint> points)
    {
        Sort = sort;
        Title = title;
        BasePrice = basePrice;
        Summary = summary;
        Foot = foot;
        _points.AddRange(points);
    }

    public int Id { get; private set; }
    public int Sort { get; private set; }
    public string Title { get; private set; }
    public decimal BasePrice { get; private set; }
    public string Summary { get; private set; }
    public string Foot { get; private set; }
    public IReadOnlyList<PriceTimelinePoint> Points => _points;

    /// <summary>Bar width percent for a point, with a visible minimum.</summary>
    public int BarPercent(PriceTimelinePoint point)
    {
        var range = _points.Max(p => p.Price) - BasePrice;
        return range <= 0 ? 100 : Math.Max(6, (int)Math.Round((point.Price - BasePrice) / range * 100, MidpointRounding.AwayFromZero));
    }
}
