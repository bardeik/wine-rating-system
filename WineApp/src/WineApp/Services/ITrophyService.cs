using WineApp.Models;

namespace WineApp.Services;

public interface ITrophyService
{
    /// <summary>
    /// Finds "Årets Vinbonde" - highest score in Group A1 with Vinbonde status
    /// </summary>
    (Wine? wine, WineResult? result) GetAaretsVinbonde(string eventId);
    
    /// <summary>
    /// Finds "Vinskuets beste norske vin" - highest score in A1, B, C, D
    /// </summary>
    (Wine? wine, WineResult? result) GetBestNorwegianWine(string eventId);
    
    /// <summary>
    /// Finds "Vinskuets beste nordiske vin" - highest score in A1 and A2
    /// </summary>
    (Wine? wine, WineResult? result) GetBestNordicWine(string eventId);
    
    /// <summary>
    /// Resolves tie-breaks: returns wine with highest single judge score
    /// If still tied, marks as requiring lottery
    /// </summary>
    List<(Wine wine, WineResult result, bool requiresLottery)> ResolveTieBreaks(
        List<(Wine wine, WineResult result)> candidates);
}
