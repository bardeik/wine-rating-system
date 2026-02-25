using WineApp.Models;

namespace WineApp.Services;

public interface IOutlierDetectionService
{
    /// <summary>
    /// Identifies wines that require re-judging due to high score spread (>4.0)
    /// </summary>
    List<(Wine wine, WineResult result, decimal spread)> GetOutlierWines(string eventId);
    
    /// <summary>
    /// Checks if a specific wine requires re-judging
    /// </summary>
    bool RequiresReJudging(string wineId, decimal outlierThreshold);
    
    /// <summary>
    /// Analyzes judge score patterns to identify potential issues
    /// </summary>
    Dictionary<string, List<string>> AnalyzeJudgePatterns(string eventId);
}
