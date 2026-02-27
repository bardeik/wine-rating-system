using WineApp.Models;

namespace WineApp.Services;

public interface IOutlierDetectionService
{
    /// <summary>
    /// Identifies wines that require re-judging due to high score spread (>4.0)
    /// </summary>
    Task<List<(Wine wine, WineResult result, decimal spread)>> GetOutlierWinesAsync(string eventId);
    
    /// <summary>
    /// Checks if a specific wine requires re-judging
    /// </summary>
    Task<bool> RequiresReJudgingAsync(string wineId, decimal outlierThreshold);
    
    /// <summary>
    /// Analyzes judge score patterns to identify potential issues
    /// </summary>
    Task<Dictionary<string, List<string>>> AnalyzeJudgePatternsAsync(string eventId);
}
