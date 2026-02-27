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

    public async Task<(Wine? wine, WineResult? result)> GetAaretsVinbondeAsync(string eventId)
    {
        // Group A1, Vinbonde status, medal classification, highest score
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines.Where(w => w.EventId == eventId && w.Group == WineGroup.A1 && w.IsVinbonde);

        var candidates = await FindMedalCandidatesAsync(wines);
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
                    await _wineResultRepository.UpdateWineResultAsync(candidate.result);
                }
            }

            return (finalCandidates.First().wine, finalCandidates.First().result);
        }

        return (winner.wine, winner.result);
    }

    public async Task<(Wine? wine, WineResult? result)> GetBestNorwegianWineAsync(string eventId)
    {
        // Groups A1, B, C, D (Norwegian wines), medal classification, highest score
        WineGroup[] norwegianGroups = [WineGroup.A1, WineGroup.B, WineGroup.C, WineGroup.D];
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines.Where(w => w.EventId == eventId && norwegianGroups.Contains(w.Group));

        return await GetTopMedalWineAsync(wines);
    }

    public async Task<(Wine? wine, WineResult? result)> GetBestNordicWineAsync(string eventId)
    {
        // Groups A1 and A2 (Norwegian + Nordic guests), medal classification, highest score
        WineGroup[] nordicGroups = [WineGroup.A1, WineGroup.A2];
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines.Where(w => w.EventId == eventId && nordicGroups.Contains(w.Group));

        return await GetTopMedalWineAsync(wines);
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

    private async Task<(Wine? wine, WineResult? result)> GetTopMedalWineAsync(IEnumerable<Wine> wines)
    {
        var candidates = await FindMedalCandidatesAsync(wines);
        return candidates.Count > 0 ? (candidates[0].wine, candidates[0].result) : (null, null);
    }

    private async Task<List<(Wine wine, WineResult result)>> FindMedalCandidatesAsync(IEnumerable<Wine> wines)
    {
        var wineList = wines.ToList();
        var allResults = await _wineResultRepository.GetAllWineResultsAsync();
        var resultLookup = allResults
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
