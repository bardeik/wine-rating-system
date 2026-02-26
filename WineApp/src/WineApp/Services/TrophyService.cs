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
            .Where(w => w.EventId == eventId && w.Group == WineGroup.A1 && w.IsVinbonde);

        var candidates = FindMedalCandidates(wines);
        if (!candidates.Any())
            return (null, null);

        var winner = candidates.First();

        // Check for tie on TotalScore
        var topScore = winner.result.TotalScore;
        var tiedCandidates = candidates.Where(x => x.result.TotalScore == topScore).ToList();

        if (tiedCandidates.Count > 1)
        {
            // Use highest single score as tie-breaker
            var topSingleScore = tiedCandidates.Max(x => x.result.HighestSingleScore);
            var finalCandidates = tiedCandidates.Where(x => x.result.HighestSingleScore == topSingleScore).ToList();

            if (finalCandidates.Count > 1)
            {
                foreach (var candidate in finalCandidates)
                {
                    candidate.result.RequiresLottery = true;
                    _wineResultRepository.UpdateWineResult(candidate.result);
                }
            }

            return (finalCandidates.First().wine, finalCandidates.First().result);
        }

        return (winner.wine, winner.result);
    }

    public (Wine? wine, WineResult? result) GetBestNorwegianWine(string eventId)
    {
        // Groups A1, B, C, D (Norwegian wines), medal classification, highest score
        WineGroup[] norwegianGroups = [WineGroup.A1, WineGroup.B, WineGroup.C, WineGroup.D];
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && norwegianGroups.Contains(w.Group));

        return GetTopMedalWine(wines);
    }

    public (Wine? wine, WineResult? result) GetBestNordicWine(string eventId)
    {
        // Groups A1 and A2 (Norwegian + Nordic guests), medal classification, highest score
        WineGroup[] nordicGroups = [WineGroup.A1, WineGroup.A2];
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && nordicGroups.Contains(w.Group));

        return GetTopMedalWine(wines);
    }

    public List<(Wine wine, WineResult result, bool requiresLottery)> ResolveTieBreaks(
        List<(Wine wine, WineResult result)> candidates)
    {
        if (!candidates.Any())
            return [];

        var topScore = candidates.Max(c => c.result.TotalScore);
        var tiedCandidates = candidates
            .Where(c => c.result.TotalScore == topScore)
            .ToList();

        if (tiedCandidates.Count == 1)
            return [(tiedCandidates[0].wine, tiedCandidates[0].result, false)];

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

    private (Wine? wine, WineResult? result) GetTopMedalWine(IEnumerable<Wine> wines)
    {
        var candidates = FindMedalCandidates(wines);
        return candidates.Count > 0 ? (candidates[0].wine, candidates[0].result) : (null, null);
    }

    private List<(Wine wine, WineResult result)> FindMedalCandidates(IEnumerable<Wine> wines)
    {
        var wineList = wines.ToList();
        var resultLookup = _wineResultRepository.GetAllWineResults()
            .Where(r => wineList.Any(w => w.WineId == r.WineId))
            .ToDictionary(r => r.WineId);

        return wineList
            .Where(w => resultLookup.TryGetValue(w.WineId, out var r)
                        && Classification.MedalClassifications.Contains(r.Classification))
            .Select(w => (w, resultLookup[w.WineId]))
            .OrderByDescending(x => x.Item2.TotalScore)
            .ThenByDescending(x => x.Item2.HighestSingleScore)
            .ToList();
    }
}
