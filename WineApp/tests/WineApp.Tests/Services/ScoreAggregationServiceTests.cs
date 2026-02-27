using FluentAssertions;
using Moq;
using WineApp.Data;
using WineApp.Models;
using WineApp.Services;

namespace WineApp.Tests.Services;

public class ScoreAggregationServiceTests
{
    private readonly Mock<IWineRatingRepository> _wineRatingRepo = new();
    private readonly Mock<IWineRepository> _wineRepo = new();
    private readonly Mock<IEventRepository> _eventRepo = new();
    private readonly Mock<IWineResultRepository> _wineResultRepo = new();
    private readonly ClassificationService _classificationService = new();

    private ScoreAggregationService CreateSut() => new(
        _wineRatingRepo.Object,
        _wineRepo.Object,
        _eventRepo.Object,
        _wineResultRepo.Object,
        _classificationService);

    private static Event DefaultEvent() => new()
    {
        EventId = "event-1",
        GoldThreshold = 17.0m,
        SilverThreshold = 15.5m,
        BronzeThreshold = 14.0m,
        SpecialMeritThreshold = 12.0m,
        AdjustedGoldThreshold = 15.0m,
        AdjustedSilverThreshold = 14.0m,
        AdjustedBronzeThreshold = 13.0m,
        AdjustedSpecialMeritThreshold = 11.5m,
        AppearanceGateValue = 1.8m,
        NoseGateValue = 1.8m,
        TasteGateValue = 5.8m,
        OutlierThreshold = 4.0m,
        UseAdjustedThresholds = false
    };

    // ── CalculateWineResult ───────────────────────────────────────

    [Fact]
    public void CalculateWineResult_NoRatings_ReturnsNotApprovedWithZeroCount()
    {
        var result = CreateSut().CalculateWineResult("wine-1", DefaultEvent(), []);

        result.WineId.Should().Be("wine-1");
        result.Classification.Should().Be(Classification.NotApproved);
        result.NumberOfRatings.Should().Be(0);
    }

    [Fact]
    public void CalculateWineResult_DefectiveRating_ReturnsNotApproved()
    {
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 0m, Nose = 3.5m, Taste = 10m },
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 2.5m, Nose = 3.5m, Taste = 11m }
        };

        var result = CreateSut().CalculateWineResult("wine-1", DefaultEvent(), ratings);

        result.IsDefective.Should().BeTrue();
        result.Classification.Should().Be(Classification.NotApproved);
    }

    [Fact]
    public void CalculateWineResult_TasteAtOrBelowOne_MarksDefective()
    {
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 1.0m }
        };

        var result = CreateSut().CalculateWineResult("wine-1", DefaultEvent(), ratings);

        result.IsDefective.Should().BeTrue();
    }

    [Fact]
    public void CalculateWineResult_BelowGateValues_ReturnsNotApproved()
    {
        // Avg appearance 1.5 < gate 1.8
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 1.5m, Nose = 3.5m, Taste = 10m },
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 1.5m, Nose = 3.5m, Taste = 10m }
        };

        var result = CreateSut().CalculateWineResult("wine-1", DefaultEvent(), ratings);

        result.MeetsGateValues.Should().BeFalse();
        result.Classification.Should().Be(Classification.NotApproved);
    }

    [Fact]
    public void CalculateWineResult_GoldScoreAllCriteriaMet_ReturnsGold()
    {
        // Total avg = 2.5 + 3.5 + 11.5 = 17.5 → Gold
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 11.5m },
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 2.5m, Nose = 3.5m, Taste = 11.5m }
        };

        var result = CreateSut().CalculateWineResult("wine-1", DefaultEvent(), ratings);

        result.Classification.Should().Be(Classification.Gold);
        result.NumberOfRatings.Should().Be(2);
        result.IsDefective.Should().BeFalse();
        result.MeetsGateValues.Should().BeTrue();
    }

    [Fact]
    public void CalculateWineResult_SpreadAboveThreshold_FlagsAsOutlier()
    {
        // Spread = 16 - 10 = 6 > threshold 4
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 10m },  // Total = 16
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 1.5m, Nose = 2.5m, Taste = 6m }    // Total = 10
        };

        var result = CreateSut().CalculateWineResult("wine-1", DefaultEvent(), ratings);

        result.IsOutlier.Should().BeTrue();
        result.Spread.Should().BeGreaterThan(4.0m);
    }

    [Fact]
    public void CalculateWineResult_SpreadBelowThreshold_NotAnOutlier()
    {
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 10m },  // Total = 16
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 2.5m, Nose = 3.5m, Taste = 10.5m } // Total = 16.5
        };

        var result = CreateSut().CalculateWineResult("wine-1", DefaultEvent(), ratings);

        result.IsOutlier.Should().BeFalse();
    }

    [Fact]
    public void CalculateWineResult_AveragesAreRoundedToOneDecimalPlace()
    {
        // Appearance avg = (2.0 + 2.5) / 2 = 2.25 → rounded to 2.3 (banker's rounding → 2.2 for midpoints)
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.0m, Nose = 3.0m, Taste = 9m },
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 2.5m, Nose = 3.5m, Taste = 10m }
        };

        var result = CreateSut().CalculateWineResult("wine-1", DefaultEvent(), ratings);

        result.AverageAppearance.Should().Be(Math.Round(2.25m, 1));
        result.AverageNose.Should().Be(Math.Round(3.25m, 1));
        result.AverageTaste.Should().Be(Math.Round(9.5m, 1));
    }

    // ── GetHighestSingleScore ─────────────────────────────────────

    [Fact]
    public void GetHighestSingleScore_EmptyList_ReturnsZeroAndEmptyJudge()
    {
        var (score, judgeId) = CreateSut().GetHighestSingleScore([]);

        score.Should().Be(0);
        judgeId.Should().BeEmpty();
    }

    [Fact]
    public void GetHighestSingleScore_MultipleRatings_ReturnsHighest()
    {
        var ratings = new List<WineRating>
        {
            new() { JudgeId = "judge-1", Appearance = 2.0m, Nose = 3.0m, Taste = 10m }, // Total = 15
            new() { JudgeId = "judge-2", Appearance = 2.5m, Nose = 3.5m, Taste = 12m }, // Total = 18
            new() { JudgeId = "judge-3", Appearance = 1.5m, Nose = 2.5m, Taste = 8m  }  // Total = 12
        };

        var (score, judgeId) = CreateSut().GetHighestSingleScore(ratings);

        score.Should().Be(18m);
        judgeId.Should().Be("judge-2");
    }

    [Fact]
    public void GetHighestSingleScore_SingleRating_ReturnsThatRating()
    {
        var ratings = new List<WineRating>
        {
            new() { JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 11m } // Total = 17
        };

        var (score, judgeId) = CreateSut().GetHighestSingleScore(ratings);

        score.Should().Be(17m);
        judgeId.Should().Be("judge-1");
    }

    // ── CalculateSpread ───────────────────────────────────────────

    [Fact]
    public void CalculateSpread_EmptyList_ReturnsZero()
    {
        var result = CreateSut().CalculateSpread([]);

        result.Should().Be(0);
    }

    [Fact]
    public void CalculateSpread_AllSameScores_ReturnsZero()
    {
        var ratings = new List<WineRating>
        {
            new() { Appearance = 2.0m, Nose = 3.0m, Taste = 9m }, // 14
            new() { Appearance = 2.0m, Nose = 3.0m, Taste = 9m }  // 14
        };

        var result = CreateSut().CalculateSpread(ratings);

        result.Should().Be(0);
    }

    [Fact]
    public void CalculateSpread_DifferentScores_ReturnsMaxMinusMins()
    {
        var ratings = new List<WineRating>
        {
            new() { Appearance = 2.0m, Nose = 3.0m, Taste = 9m  }, // 14
            new() { Appearance = 2.5m, Nose = 3.5m, Taste = 12m }, // 18
            new() { Appearance = 1.5m, Nose = 2.0m, Taste = 7m  }  // 10.5
        };

        var result = CreateSut().CalculateSpread(ratings);

        result.Should().Be(Math.Round(18m - 10.5m, 1));
    }

    [Fact]
    public void CalculateSpread_SingleRating_ReturnsZero()
    {
        var ratings = new List<WineRating>
        {
            new() { Appearance = 2.5m, Nose = 3.5m, Taste = 11m }
        };

        var result = CreateSut().CalculateSpread(ratings);

        result.Should().Be(0);
    }

    // ── RecalculateEventResultsAsync ──────────────────────────────

    [Fact]
    public async Task RecalculateEventResultsAsync_EventNotFound_ThrowsInvalidOperationException()
    {
        _eventRepo.Setup(r => r.GetEventByIdAsync("missing")).ReturnsAsync((Event?)null);

        await CreateSut().Invoking(s => s.RecalculateEventResultsAsync("missing"))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RecalculateEventResultsAsync_WinesInEvent_CreatesResults()
    {
        var eventConfig = DefaultEvent();
        _eventRepo.Setup(r => r.GetEventByIdAsync("event-1")).ReturnsAsync(eventConfig);

        var wine = new Wine { WineId = "wine-1", EventId = "event-1" };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine]);

        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 11.5m }
        };
        _wineRatingRepo.Setup(r => r.GetAllWineRatingsAsync()).ReturnsAsync(ratings);
        _wineResultRepo.Setup(r => r.GetWineResultByWineIdAsync("wine-1")).ReturnsAsync((WineResult?)null);
        _wineResultRepo.Setup(r => r.AddWineResultAsync(It.IsAny<WineResult>())).Returns(Task.CompletedTask);

        var results = await CreateSut().RecalculateEventResultsAsync("event-1");

        results.Should().HaveCount(1);
        results[0].WineId.Should().Be("wine-1");
        _wineResultRepo.Verify(r => r.AddWineResultAsync(It.IsAny<WineResult>()), Times.Once);
    }

    [Fact]
    public async Task RecalculateEventResultsAsync_ExistingResult_UpdatesInsteadOfAdding()
    {
        var eventConfig = DefaultEvent();
        _eventRepo.Setup(r => r.GetEventByIdAsync("event-1")).ReturnsAsync(eventConfig);

        var wine = new Wine { WineId = "wine-1", EventId = "event-1" };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine]);
        _wineRatingRepo.Setup(r => r.GetAllWineRatingsAsync()).ReturnsAsync([]);

        var existingResult = new WineResult { WineResultId = "result-1", WineId = "wine-1" };
        _wineResultRepo.Setup(r => r.GetWineResultByWineIdAsync("wine-1")).ReturnsAsync(existingResult);
        _wineResultRepo.Setup(r => r.UpdateWineResultAsync(It.IsAny<WineResult>())).Returns(Task.CompletedTask);

        await CreateSut().RecalculateEventResultsAsync("event-1");

        _wineResultRepo.Verify(r => r.UpdateWineResultAsync(It.IsAny<WineResult>()), Times.Once);
        _wineResultRepo.Verify(r => r.AddWineResultAsync(It.IsAny<WineResult>()), Times.Never);
    }
}
