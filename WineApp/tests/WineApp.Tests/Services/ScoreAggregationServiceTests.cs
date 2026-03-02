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

    private ScoreAggregationService CreateSut(TimeProvider? timeProvider = null) => new(
        _wineRatingRepo.Object,
        _wineRepo.Object,
        _eventRepo.Object,
        _wineResultRepo.Object,
        _classificationService,
        timeProvider ?? TimeProvider.System);

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

        result.WineId.ShouldBe("wine-1");
        result.Classification.ShouldBe(Classification.NotApproved);
        result.NumberOfRatings.ShouldBe(0);
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

        result.IsDefective.ShouldBeTrue();
        result.Classification.ShouldBe(Classification.NotApproved);
    }

    [Fact]
    public void CalculateWineResult_TasteAtOrBelowOne_MarksDefective()
    {
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 1.0m }
        };

        var result = CreateSut().CalculateWineResult("wine-1", DefaultEvent(), ratings);

        result.IsDefective.ShouldBeTrue();
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

        result.MeetsGateValues.ShouldBeFalse();
        result.Classification.ShouldBe(Classification.NotApproved);
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

        result.Classification.ShouldBe(Classification.Gold);
        result.NumberOfRatings.ShouldBe(2);
        result.IsDefective.ShouldBeFalse();
        result.MeetsGateValues.ShouldBeTrue();
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

        result.IsOutlier.ShouldBeTrue();
        result.Spread.ShouldBeGreaterThan(4.0m);
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

        result.IsOutlier.ShouldBeFalse();
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

        result.AverageAppearance.ShouldBe(Math.Round(2.25m, 1));
        result.AverageNose.ShouldBe(Math.Round(3.25m, 1));
        result.AverageTaste.ShouldBe(Math.Round(9.5m, 1));
    }

    [Fact]
    public void CalculateWineResult_CalculationDateReflectsInjectedClock()
    {
        var frozenTime = FrozenTimeProvider.At(2026, 3, 15, 14, 30);
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 11.5m }
        };

        var result = CreateSut(frozenTime).CalculateWineResult("wine-1", DefaultEvent(), ratings);

        result.CalculationDate.ShouldBe(new DateTime(2026, 3, 15, 14, 30, 0, DateTimeKind.Utc));
    }

    // ── GetHighestSingleScore ─────────────────────────────────────

    [Fact]
    public void GetHighestSingleScore_EmptyList_ReturnsZeroAndEmptyJudge()
    {
        var (score, judgeId) = CreateSut().GetHighestSingleScore([]);

        score.ShouldBe(0);
        judgeId.ShouldBeEmpty();
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

        score.ShouldBe(18m);
        judgeId.ShouldBe("judge-2");
    }

    [Fact]
    public void GetHighestSingleScore_SingleRating_ReturnsThatRating()
    {
        var ratings = new List<WineRating>
        {
            new() { JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 11m } // Total = 17
        };

        var (score, judgeId) = CreateSut().GetHighestSingleScore(ratings);

        score.ShouldBe(17m);
        judgeId.ShouldBe("judge-1");
    }

    // ── CalculateSpread ───────────────────────────────────────────

    [Fact]
    public void CalculateSpread_EmptyList_ReturnsZero()
    {
        var result = CreateSut().CalculateSpread([]);

        result.ShouldBe(0);
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

        result.ShouldBe(0);
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

        result.ShouldBe(Math.Round(18m - 10.5m, 1));
    }

    [Fact]
    public void CalculateSpread_SingleRating_ReturnsZero()
    {
        var ratings = new List<WineRating>
        {
            new() { Appearance = 2.5m, Nose = 3.5m, Taste = 11m }
        };

        var result = CreateSut().CalculateSpread(ratings);

        result.ShouldBe(0);
    }

    // ── RecalculateEventResultsAsync ──────────────────────────────

    [Fact]
    public async Task RecalculateEventResultsAsync_EventNotFound_ThrowsInvalidOperationException()
    {
        _eventRepo.Setup(r => r.GetEventByIdAsync("missing")).ReturnsAsync((Event?)null);

        await Should.ThrowAsync<InvalidOperationException>(() => CreateSut().RecalculateEventResultsAsync("missing"));
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

        results.Count.ShouldBe(1);
        results[0].WineId.ShouldBe("wine-1");
        _wineResultRepo.Verify(r => r.AddWineResultAsync(It.IsAny<WineResult>()), Times.Once);
    }

    // ── Endrede grenseverdier end-to-end ─────────────────────────

    [Fact]
    public void CalculateWineResult_CustomRaisedGoldThreshold_WineThatWasGoldBecomeSilver()
    {
        // Score 17.5 → Gull med standard. Admin hever Gull til 18.0 → skal bli Sølv.
        var eventConfig = DefaultEvent();
        eventConfig.GoldThreshold = 18.0m;

        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 11.5m }, // 17.5
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 2.5m, Nose = 3.5m, Taste = 11.5m }  // 17.5
        };

        var result = CreateSut().CalculateWineResult("wine-1", eventConfig, ratings);

        result.TotalScore.ShouldBe(17.5m);
        result.Classification.ShouldBe(Classification.Silver);
    }

    [Fact]
    public void CalculateWineResult_CustomLoweredGoldThreshold_WineThatWasSilverBecomesGold()
    {
        // Score 15.5 → Sølv med standard. Admin senker Gull til 15.0 → skal bli Gull.
        var eventConfig = DefaultEvent();
        eventConfig.GoldThreshold = 15.0m;

        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.0m, Nose = 2.0m, Taste = 11.5m }, // 15.5
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 2.0m, Nose = 2.0m, Taste = 11.5m }  // 15.5
        };

        var result = CreateSut().CalculateWineResult("wine-1", eventConfig, ratings);

        result.TotalScore.ShouldBe(15.5m);
        result.Classification.ShouldBe(Classification.Gold);
    }

    [Fact]
    public void CalculateWineResult_CustomRaisedAppearanceGateValue_WineFailsGate()
    {
        // Appearance avg = 1.6; standard gate 1.8 → underkjent. Admin hever til 1.7 → fortsatt underkjent.
        var eventConfig = DefaultEvent();
        eventConfig.AppearanceGateValue = 1.7m;

        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 1.6m, Nose = 3.5m, Taste = 11.5m },
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 1.6m, Nose = 3.5m, Taste = 11.5m }
        };

        var result = CreateSut().CalculateWineResult("wine-1", eventConfig, ratings);

        result.MeetsGateValues.ShouldBeFalse();
        result.Classification.ShouldBe(Classification.NotApproved);
    }

    [Fact]
    public void CalculateWineResult_CustomLoweredAppearanceGateValue_PreviouslyFailingWineNowPasses()
    {
        // Appearance avg = 1.6; admin senker gate til 1.5 → passerer, score 15.1 → Bronse (15.1 < Sølv 15.5)
        var eventConfig = DefaultEvent();
        eventConfig.AppearanceGateValue = 1.5m;

        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 1.6m, Nose = 3.5m, Taste = 10m }, // 15.1
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 1.6m, Nose = 3.5m, Taste = 10m }  // 15.1
        };

        var result = CreateSut().CalculateWineResult("wine-1", eventConfig, ratings);

        result.MeetsGateValues.ShouldBeTrue();
        result.Classification.ShouldBe(Classification.Bronze);
    }

    [Fact]
    public void CalculateWineResult_CustomAdjustedThresholdsEnabled_UsesAdjustedValues()
    {
        // Score 15.5 → Sølv med ordinære standardverdier.
        // Admin aktiverer nedjustert modus med AdjustedGold = 15.0 → skal bli Gull.
        var eventConfig = DefaultEvent();
        eventConfig.UseAdjustedThresholds = true;
        eventConfig.AdjustedGoldThreshold = 15.0m;

        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.0m, Nose = 2.0m, Taste = 11.5m }, // 15.5
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 2.0m, Nose = 2.0m, Taste = 11.5m }  // 15.5
        };

        var result = CreateSut().CalculateWineResult("wine-1", eventConfig, ratings);

        result.TotalScore.ShouldBe(15.5m);
        result.Classification.ShouldBe(Classification.Gold);
    }

    [Fact]
    public void CalculateWineResult_CustomOutlierThreshold_HigherThresholdSuppressesOutlierFlag()
    {
        // Spread = 6.0; standard terskel 4.0 → utliggerflagg. Admin hever til 7.0 → ingen flagg.
        var eventConfig = DefaultEvent();
        eventConfig.OutlierThreshold = 7.0m;

        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 10m },  // 16
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 1.5m, Nose = 2.5m, Taste = 6m }    // 10
        };

        var result = CreateSut().CalculateWineResult("wine-1", eventConfig, ratings);

        result.Spread.ShouldBe(6.0m);
        result.IsOutlier.ShouldBeFalse();
    }

    [Fact]
    public async Task RecalculateEventResultsAsync_WithCustomThresholds_ClassifiesCorrectly()
    {
        // Admin har hevet Gull til 18.0; score 17.5 skal gi Sølv i databasen
        var eventConfig = DefaultEvent();
        eventConfig.GoldThreshold = 18.0m;

        _eventRepo.Setup(r => r.GetEventByIdAsync("event-1")).ReturnsAsync(eventConfig);

        var wine = new Wine { WineId = "wine-1", EventId = "event-1" };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine]);

        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 11.5m }, // 17.5
            new() { WineId = "wine-1", JudgeId = "judge-2", Appearance = 2.5m, Nose = 3.5m, Taste = 11.5m }  // 17.5
        };
        _wineRatingRepo.Setup(r => r.GetAllWineRatingsAsync()).ReturnsAsync(ratings);
        _wineResultRepo.Setup(r => r.GetWineResultByWineIdAsync("wine-1")).ReturnsAsync((WineResult?)null);
        _wineResultRepo.Setup(r => r.AddWineResultAsync(It.IsAny<WineResult>())).Returns(Task.CompletedTask);

        var results = await CreateSut().RecalculateEventResultsAsync("event-1");

        results[0].TotalScore.ShouldBe(17.5m);
        results[0].Classification.ShouldBe(Classification.Silver);
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
