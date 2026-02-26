using WineApp.Models;

namespace WineApp.Services;

public interface IScoreAggregationService
{
    /// <summary>
    /// Calculates aggregated results for a wine based on all judge ratings
    /// </summary>
    WineResult CalculateWineResult(string wineId, Event eventConfig, List<WineRating> ratings);
    
    /// <summary>
    /// Recalculates results for all wines in an event
    /// </summary>
    List<WineResult> RecalculateEventResults(string eventId);
    
    /// <summary>
    /// Gets the highest single score from any judge for a wine
    /// </summary>
    (decimal highestScore, string judgeId) GetHighestSingleScore(List<WineRating> ratings);
    
    /// <summary>
    /// Calculates the spread (max - min total) across all judge ratings
    /// </summary>
    decimal CalculateSpread(List<WineRating> ratings);
}
