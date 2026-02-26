using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class TrophyService : ITrophyService
{
    private readonly IWineRepository _wineRepository;
    private readonly IWineResultRepository _wineResultRepository;

    public TrophyService(
        IWineRepository wineRepository,
        IWineResultRepository wineResultRepository)
    {
        _wineRepository = wineRepository;
        _wineResultRepository = wineResultRepository;
    }

    public (Wine? wine, WineResult? result) GetAaretsVinbonde(string eventId)
    {
        // Group A1, Vinbonde status, medal classification, highest score
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && 
                       w.Group == WineGroup.A1 && 
                       w.IsVinbonde)
            .ToList();

        var results = _wineResultRepository.GetAllWineResults();

        var candidates = wines
            .Select(w => new
            {
                Wine = w,
                Result = results.FirstOrDefault(r => r.WineId == w.WineId)
            })
            .Where(x => x.Result != null && Classification.MedalClassifications.Contains(x.Result.Classification))
            .OrderByDescending(x => x.Result!.TotalScore)
            .ThenByDescending(x => x.Result!.HighestSingleScore)
            .ToList();

        if (!candidates.Any())
            return (null, null);

        var winner = candidates.First();
        
        // Check for tie
        var topScore = winner.Result!.TotalScore;
        var tiedCandidates = candidates
            .Where(x => x.Result!.TotalScore == topScore)
            .ToList();

        if (tiedCandidates.Count > 1)
        {
            // Use highest single score as tie-breaker
            var topSingleScore = tiedCandidates.Max(x => x.Result!.HighestSingleScore);
            var finalCandidates = tiedCandidates
                .Where(x => x.Result!.HighestSingleScore == topSingleScore)
                .ToList();

            if (finalCandidates.Count > 1)
            {
                // Mark as requiring lottery
                foreach (var candidate in finalCandidates)
                {
                    candidate.Result!.RequiresLottery = true;
                    _wineResultRepository.UpdateWineResult(candidate.Result);
                }
            }

            return (finalCandidates.First().Wine, finalCandidates.First().Result);
        }

        return (winner.Wine, winner.Result);
    }

    public (Wine? wine, WineResult? result) GetBestNorwegianWine(string eventId)
    {
        // Groups A1, B, C, D (Norwegian wines), medal classification, highest score
        var norwegianGroups = new[] { WineGroup.A1, WineGroup.B, WineGroup.C, WineGroup.D };
        
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && 
                       norwegianGroups.Contains(w.Group))
            .ToList();

        var results = _wineResultRepository.GetAllWineResults();

        var candidates = wines
            .Select(w => new
            {
                Wine = w,
                Result = results.FirstOrDefault(r => r.WineId == w.WineId)
            })
            .Where(x => x.Result != null && Classification.MedalClassifications.Contains(x.Result.Classification))
            .OrderByDescending(x => x.Result!.TotalScore)
            .ThenByDescending(x => x.Result!.HighestSingleScore)
            .ToList();

        if (!candidates.Any())
            return (null, null);

        var winner = candidates.First();
        return (winner.Wine, winner.Result);
    }

    public (Wine? wine, WineResult? result) GetBestNordicWine(string eventId)
    {
        // Groups A1 and A2 (Norwegian + Nordic guests), medal classification, highest score
        var nordicGroups = new[] { WineGroup.A1, WineGroup.A2 };
        
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && 
                       nordicGroups.Contains(w.Group))
            .ToList();

        var results = _wineResultRepository.GetAllWineResults();

        var candidates = wines
            .Select(w => new
            {
                Wine = w,
                Result = results.FirstOrDefault(r => r.WineId == w.WineId)
            })
            .Where(x => x.Result != null && Classification.MedalClassifications.Contains(x.Result.Classification))
            .OrderByDescending(x => x.Result!.TotalScore)
            .ThenByDescending(x => x.Result!.HighestSingleScore)
            .ToList();

        if (!candidates.Any())
            return (null, null);

        var winner = candidates.First();
        return (winner.Wine, winner.Result);
    }

    public List<(Wine wine, WineResult result, bool requiresLottery)> ResolveTieBreaks(
        List<(Wine wine, WineResult result)> candidates)
    {
        if (!candidates.Any())
            return new List<(Wine, WineResult, bool)>();

        var topScore = candidates.Max(c => c.result.TotalScore);
        var tiedCandidates = candidates
            .Where(c => c.result.TotalScore == topScore)
            .ToList();

        if (tiedCandidates.Count == 1)
            return new List<(Wine, WineResult, bool)> { (tiedCandidates[0].wine, tiedCandidates[0].result, false) };

        // Apply tie-break: highest single judge score
        var topSingleScore = tiedCandidates.Max(c => c.result.HighestSingleScore);
        var finalCandidates = tiedCandidates
            .Where(c => c.result.HighestSingleScore == topSingleScore)
            .ToList();

        var requiresLottery = finalCandidates.Count > 1;

        return finalCandidates
            .Select(c => (c.wine, c.result, requiresLottery))
            .ToList();
    }
}
