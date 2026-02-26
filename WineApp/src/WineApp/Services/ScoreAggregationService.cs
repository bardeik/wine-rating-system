using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class ScoreAggregationService : IScoreAggregationService
{
    private readonly IWineRatingRepository _wineRatingRepository;
    private readonly IWineRepository _wineRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IWineResultRepository _wineResultRepository;
    private readonly IClassificationService _classificationService;

    public ScoreAggregationService(
        IWineRatingRepository wineRatingRepository,
        IWineRepository wineRepository,
        IEventRepository eventRepository,
        IWineResultRepository wineResultRepository,
        IClassificationService classificationService)
    {
        _wineRatingRepository = wineRatingRepository;
        _wineRepository = wineRepository;
        _eventRepository = eventRepository;
        _wineResultRepository = wineResultRepository;
        _classificationService = classificationService;
    }

    public WineResult CalculateWineResult(string wineId, Event eventConfig, List<WineRating> ratings)
    {
        if (!ratings.Any())
        {
            return new WineResult
            {
                WineId = wineId,
                Classification = Classification.NotApproved,
                NumberOfRatings = 0
            };
        }

        // Calculate panel averages
        var avgAppearance = Math.Round(ratings.Average(r => r.Appearance), 1);
        var avgNose = Math.Round(ratings.Average(r => r.Nose), 1);
        var avgTaste = Math.Round(ratings.Average(r => r.Taste), 1);
        var totalScore = Math.Round(avgAppearance + avgNose + avgTaste, 1);

        // Check for defects (any judge gave 0 on Appearance or Nose, or <=1 on Taste)
        var isDefective = ratings.Any(r => 
            r.Appearance == 0 || 
            r.Nose == 0 || 
            r.Taste <= 1.0m);

        // Check gate values
        var meetsGateValues = 
            avgAppearance >= eventConfig.AppearanceGateValue &&
            avgNose >= eventConfig.NoseGateValue &&
            avgTaste >= eventConfig.TasteGateValue;

        // Calculate spread
        var spread = CalculateSpread(ratings);
        var isOutlier = spread > eventConfig.OutlierThreshold;

        // Get highest single score
        var (highestScore, judgeId) = GetHighestSingleScore(ratings);

        // Determine classification
        var classification = _classificationService.ClassifyWine(
            totalScore, 
            avgAppearance, 
            avgNose, 
            avgTaste, 
            isDefective, 
            meetsGateValues, 
            eventConfig);

        return new WineResult
        {
            WineId = wineId,
            AverageAppearance = avgAppearance,
            AverageNose = avgNose,
            AverageTaste = avgTaste,
            TotalScore = totalScore,
            Classification = classification,
            IsDefective = isDefective,
            IsOutlier = isOutlier,
            Spread = spread,
            HighestSingleScore = highestScore,
            HighestScoreJudgeId = judgeId,
            MeetsGateValues = meetsGateValues,
            NumberOfRatings = ratings.Count,
            CalculationDate = DateTime.UtcNow
        };
    }

    public Task<List<WineResult>> RecalculateEventResultsAsync(string eventId)
    {
        var eventConfig = _eventRepository.GetEventById(eventId);
        if (eventConfig == null)
            throw new InvalidOperationException($"Event {eventId} not found");

        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId)
            .ToList();

        var allRatings = _wineRatingRepository.GetAllWineRatings();
        var results = new List<WineResult>();

        foreach (var wine in wines)
        {
            var wineRatings = allRatings.Where(r => r.WineId == wine.WineId).ToList();
            var result = CalculateWineResult(wine.WineId, eventConfig, wineRatings);

            // Save or update result
            var existingResult = _wineResultRepository.GetWineResultByWineId(wine.WineId);
            if (existingResult != null)
            {
                result.WineResultId = existingResult.WineResultId;
                _wineResultRepository.UpdateWineResult(result);
            }
            else
            {
                _wineResultRepository.AddWineResult(result);
            }

            results.Add(result);
        }

        // Check if any Gold medals awarded - if not, recalculate with adjusted thresholds
        if (!results.Any(r => r.Classification == Classification.Gold) && !eventConfig.UseAdjustedThresholds)
        {
            // Notify admin to consider adjusted thresholds
            // This could trigger a workflow or notification
        }

        return Task.FromResult(results);
    }

    public (decimal highestScore, string judgeId) GetHighestSingleScore(List<WineRating> ratings)
    {
        if (!ratings.Any())
            return (0, string.Empty);

        var highestRating = ratings.OrderByDescending(r => r.Total).First();
        return (highestRating.Total, highestRating.JudgeId);
    }

    public decimal CalculateSpread(List<WineRating> ratings)
    {
        if (!ratings.Any())
            return 0;

        var totals = ratings.Select(r => r.Total).ToList();
        return Math.Round(totals.Max() - totals.Min(), 1);
    }
}
