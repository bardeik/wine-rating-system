using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class OutlierDetectionService : IOutlierDetectionService
{
    private readonly IWineRepository _wineRepository;
    private readonly IWineResultRepository _wineResultRepository;
    private readonly IWineRatingRepository _wineRatingRepository;

    public OutlierDetectionService(
        IWineRepository wineRepository,
        IWineResultRepository wineResultRepository,
        IWineRatingRepository wineRatingRepository)
    {
        _wineRepository = wineRepository;
        _wineResultRepository = wineResultRepository;
        _wineRatingRepository = wineRatingRepository;
    }

    public async Task<List<(Wine wine, WineResult result, decimal spread)>> GetOutlierWinesAsync(string eventId)
    {
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines
            .Where(w => w.EventId == eventId)
            .ToDictionary(w => w.WineId);

        var outlierResults = await _wineResultRepository.GetOutlierWineResultsAsync();
        return outlierResults
            .Where(r => wines.ContainsKey(r.WineId))
            .Select(r => (wines[r.WineId], r, r.Spread))
            .OrderByDescending(o => o.Spread)
            .ToList();
    }

    public async Task<bool> RequiresReJudgingAsync(string wineId, decimal outlierThreshold)
    {
        var result = await _wineResultRepository.GetWineResultByWineIdAsync(wineId);
        if (result == null)
            return false;

        return result.Spread > outlierThreshold;
    }

    public async Task<Dictionary<string, List<string>>> AnalyzeJudgePatternsAsync(string eventId)
    {
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines
            .Where(w => w.EventId == eventId)
            .ToList();

        var allRatings = await _wineRatingRepository.GetAllWineRatingsAsync();
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
