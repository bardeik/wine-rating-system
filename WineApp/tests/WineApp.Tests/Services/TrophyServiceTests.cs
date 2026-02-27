using FluentAssertions;
using Moq;
using WineApp.Data;
using WineApp.Models;
using WineApp.Services;

namespace WineApp.Tests.Services;

public class TrophyServiceTests
{
    private readonly Mock<IWineRepository> _wineRepo = new();
    private readonly Mock<IWineResultRepository> _wineResultRepo = new();

    private TrophyService CreateSut() => new(_wineRepo.Object, _wineResultRepo.Object);

    // ── ResolveTieBreaks ──────────────────────────────────────────

    [Fact]
    public void ResolveTieBreaks_EmptyList_ReturnsEmpty()
    {
        var result = CreateSut().ResolveTieBreaks([]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ResolveTieBreaks_SingleCandidate_ReturnsThatCandidateNoLottery()
    {
        var wine = new Wine { WineId = "wine-1" };
        var wineResult = new WineResult { WineId = "wine-1", TotalScore = 17.5m, HighestSingleScore = 18m };

        var result = CreateSut().ResolveTieBreaks([(wine, wineResult)]);

        result.Should().HaveCount(1);
        result[0].requiresLottery.Should().BeFalse();
        result[0].wine.WineId.Should().Be("wine-1");
    }

    [Fact]
    public void ResolveTieBreaks_TieBrokenByHighestSingleScore_ReturnsWinnerNoLottery()
    {
        var wine1 = new Wine { WineId = "wine-1" };
        var wine2 = new Wine { WineId = "wine-2" };
        var result1 = new WineResult { WineId = "wine-1", TotalScore = 17.0m, HighestSingleScore = 18m };
        var result2 = new WineResult { WineId = "wine-2", TotalScore = 17.0m, HighestSingleScore = 17m };

        var result = CreateSut().ResolveTieBreaks([(wine1, result1), (wine2, result2)]);

        result.Should().HaveCount(1);
        result[0].wine.WineId.Should().Be("wine-1");
        result[0].requiresLottery.Should().BeFalse();
    }

    [Fact]
    public void ResolveTieBreaks_TieWithEqualHighestSingleScore_RequiresLottery()
    {
        var wine1 = new Wine { WineId = "wine-1" };
        var wine2 = new Wine { WineId = "wine-2" };
        var result1 = new WineResult { WineId = "wine-1", TotalScore = 17.0m, HighestSingleScore = 18m };
        var result2 = new WineResult { WineId = "wine-2", TotalScore = 17.0m, HighestSingleScore = 18m };

        var result = CreateSut().ResolveTieBreaks([(wine1, result1), (wine2, result2)]);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.requiresLottery.Should().BeTrue());
    }

    [Fact]
    public void ResolveTieBreaks_NoClearWinner_OnlyTopScoresConsidered()
    {
        var winnerWine = new Wine { WineId = "wine-winner" };
        var loserWine = new Wine { WineId = "wine-loser" };
        var winnerResult = new WineResult { WineId = "wine-winner", TotalScore = 17.0m, HighestSingleScore = 18m };
        var loserResult = new WineResult { WineId = "wine-loser", TotalScore = 16.0m, HighestSingleScore = 19m }; // higher single but lower total

        var result = CreateSut().ResolveTieBreaks([(winnerWine, winnerResult), (loserWine, loserResult)]);

        // Only the wine with the highest TotalScore should be considered
        result.Should().HaveCount(1);
        result[0].wine.WineId.Should().Be("wine-winner");
    }

    [Fact]
    public void ResolveTieBreaks_ThreeCandidatesOneTieBreakWinner_ReturnsOne()
    {
        var wine1 = new Wine { WineId = "wine-1" };
        var wine2 = new Wine { WineId = "wine-2" };
        var wine3 = new Wine { WineId = "wine-3" };
        var r1 = new WineResult { WineId = "wine-1", TotalScore = 18.0m, HighestSingleScore = 19m };
        var r2 = new WineResult { WineId = "wine-2", TotalScore = 18.0m, HighestSingleScore = 18m };
        var r3 = new WineResult { WineId = "wine-3", TotalScore = 18.0m, HighestSingleScore = 18m };

        var result = CreateSut().ResolveTieBreaks([(wine1, r1), (wine2, r2), (wine3, r3)]);

        result.Should().HaveCount(1);
        result[0].wine.WineId.Should().Be("wine-1");
        result[0].requiresLottery.Should().BeFalse();
    }

    // ── GetBestNorwegianWineAsync ─────────────────────────────────

    [Fact]
    public async Task GetBestNorwegianWineAsync_NoMedalWines_ReturnsNull()
    {
        var wines = new List<Wine>
        {
            new() { WineId = "wine-1", EventId = "event-1", Group = WineGroup.A1 }
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);
        _wineResultRepo.Setup(r => r.GetAllWineResultsAsync()).ReturnsAsync([]);

        var (wine, wineResult) = await CreateSut().GetBestNorwegianWineAsync("event-1");

        wine.Should().BeNull();
        wineResult.Should().BeNull();
    }

    [Fact]
    public async Task GetBestNorwegianWineAsync_MultipleGroupsWithMedalWine_ReturnsBestByTotalScore()
    {
        var wines = new List<Wine>
        {
            new() { WineId = "wine-a1", EventId = "event-1", Group = WineGroup.A1 },
            new() { WineId = "wine-b",  EventId = "event-1", Group = WineGroup.B },
        };
        var results = new List<WineResult>
        {
            new() { WineId = "wine-a1", TotalScore = 17.0m, Classification = Classification.Gold, HighestSingleScore = 18m },
            new() { WineId = "wine-b",  TotalScore = 16.0m, Classification = Classification.Silver, HighestSingleScore = 17m },
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);
        _wineResultRepo.Setup(r => r.GetAllWineResultsAsync()).ReturnsAsync(results);

        var (bestWine, bestResult) = await CreateSut().GetBestNorwegianWineAsync("event-1");

        bestWine!.WineId.Should().Be("wine-a1");
        bestResult!.TotalScore.Should().Be(17.0m);
    }

    [Fact]
    public async Task GetBestNorwegianWineAsync_A2WineExcluded()
    {
        var wines = new List<Wine>
        {
            new() { WineId = "wine-a2", EventId = "event-1", Group = WineGroup.A2 }, // not Norwegian
            new() { WineId = "wine-a1", EventId = "event-1", Group = WineGroup.A1 },
        };
        var results = new List<WineResult>
        {
            new() { WineId = "wine-a2", TotalScore = 20.0m, Classification = Classification.Gold, HighestSingleScore = 20m },
            new() { WineId = "wine-a1", TotalScore = 15.0m, Classification = Classification.Silver, HighestSingleScore = 16m },
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);
        _wineResultRepo.Setup(r => r.GetAllWineResultsAsync()).ReturnsAsync(results);

        var (bestWine, _) = await CreateSut().GetBestNorwegianWineAsync("event-1");

        // A2 wines should not count as Norwegian
        bestWine!.WineId.Should().Be("wine-a1");
    }

    // ── GetBestNordicWineAsync ────────────────────────────────────

    [Fact]
    public async Task GetBestNordicWineAsync_IncludesA1AndA2Groups()
    {
        var wines = new List<Wine>
        {
            new() { WineId = "wine-a2", EventId = "event-1", Group = WineGroup.A2 },
            new() { WineId = "wine-b",  EventId = "event-1", Group = WineGroup.B },
        };
        var results = new List<WineResult>
        {
            new() { WineId = "wine-a2", TotalScore = 17.0m, Classification = Classification.Gold, HighestSingleScore = 18m },
            new() { WineId = "wine-b",  TotalScore = 18.0m, Classification = Classification.Gold, HighestSingleScore = 19m },
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);
        _wineResultRepo.Setup(r => r.GetAllWineResultsAsync()).ReturnsAsync(results);

        var (bestWine, _) = await CreateSut().GetBestNordicWineAsync("event-1");

        // Only A1 and A2 are Nordic groups; Group B should not be included
        bestWine!.WineId.Should().Be("wine-a2");
    }
}
