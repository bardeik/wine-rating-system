using WineApp.Models;

namespace WineApp.Services;

public interface IClassificationService
{
    /// <summary>
    /// Classifies a wine based on total score and gate values
    /// </summary>
    string ClassifyWine(
        decimal totalScore, 
        decimal avgAppearance, 
        decimal avgNose, 
        decimal avgTaste,
        bool isDefective,
        bool meetsGateValues,
        Event eventConfig);
    
    /// <summary>
    /// Checks if adjusted thresholds should be applied (no gold medals awarded)
    /// </summary>
    bool ShouldUseAdjustedThresholds(List<WineResult> results);
    
    /// <summary>
    /// Gets the appropriate threshold for a classification level
    /// </summary>
    decimal GetThreshold(string classification, Event eventConfig);
}
