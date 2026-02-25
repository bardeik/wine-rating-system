using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class OutlierDetectionService : IOutlierDetectionService
{
    private readonly IWineRepository _wineRepository;
    private readonly IWineResultRepository _wineResultRepository;
    private readonly IWineRatingRepository _wineRatingRepository;
    private readonly IScoreAggregationService _scoreAggregationService;

    public OutlierDetectionService(
        IWineRepository wineRepository,
        IWineResultRepository wineResultRepository,
        IWineRatingRepository wineRatingRepository,
        IScoreAggregationService scoreAggregationService)
    {
        _wineRepository = wineRepository;
        _wineResultRepository = wineResultRepository;
        _wineRatingRepository = wineRatingRepository;
        _scoreAggregationService = scoreAggregationService;
    }

    public List<(Wine wine, WineResult result, decimal spread)> GetOutlierWines(string eventId)
    {
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId)
            .ToList();

        var results = _wineResultRepository.GetAllWineResults()
            .Where(r => r.IsOutlier)
            .ToList();

        var outliers = new List<(Wine, WineResult, decimal)>();

        foreach (var result in results)
        {
            var wine = wines.FirstOrDefault(w => w.WineId == result.WineId);
            if (wine != null)
            {
                outliers.Add((wine, result, result.Spread));
            }
        }

        return outliers.OrderByDescending(o => o.Item3).ToList();
    }

    public bool RequiresReJudging(string wineId, decimal outlierThreshold)
    {
        var result = _wineResultRepository.GetWineResultByWineId(wineId);
        if (result == null)
            return false;

        return result.Spread > outlierThreshold;
    }

    public Dictionary<string, List<string>> AnalyzeJudgePatterns(string eventId)
    {
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId)
            .ToList();

        var allRatings = _wineRatingRepository.GetAllWineRatings();
        var eventRatings = allRatings
            .Where(r => wines.Any(w => w.WineId == r.WineId))
            .ToList();

        var patterns = new Dictionary<string, List<string>>();

        // Group by judge
        var judgeGroups = eventRatings.GroupBy(r => r.JudgeId);

        foreach (var judgeGroup in judgeGroups)
        {
            var judgeId = judgeGroup.Key;
            var ratings = judgeGroup.ToList();
            var issues = new List<string>();

            // Check average scoring level
            var avgTotal = ratings.Average(r => r.Total);
            if (avgTotal < 8.0m)
                issues.Add($"Lavt gjennomsnitt: {avgTotal:F1}");
            else if (avgTotal > 16.0m)
                issues.Add($"Høyt gjennomsnitt: {avgTotal:F1}");

            // Check for consistent scoring (low variance might indicate lack of discrimination)
            var totals = ratings.Select(r => r.Total).ToList();
            if (totals.Any())
            {
                var variance = CalculateVariance(totals);
                if (variance < 2.0m)
                    issues.Add("Lav variasjon i poenggiving");
            }

            // Check for defect flagging rate
            var defectRate = ratings.Count(r => r.Appearance == 0 || r.Nose == 0 || r.Taste <= 1.0m) / (decimal)ratings.Count;
            if (defectRate > 0.3m)
                issues.Add($"Høy andel feilbeheftede: {defectRate:P0}");

            if (issues.Any())
                patterns[judgeId] = issues;
        }

        return patterns;
    }

    private decimal CalculateVariance(List<decimal> values)
    {
        if (!values.Any())
            return 0;

        var avg = values.Average();
        var sumOfSquares = values.Sum(v => (v - avg) * (v - avg));
        return sumOfSquares / values.Count;
    }
}
