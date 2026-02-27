using FluentAssertions;
using Moq;
using WineApp.Data;
using WineApp.Models;
using WineApp.Services;

namespace WineApp.Tests.Services;

public class OutlierDetectionServiceTests
{
    private readonly Mock<IWineRepository> _wineRepo = new();
    private readonly Mock<IWineResultRepository> _wineResultRepo = new();
    private readonly Mock<IWineRatingRepository> _wineRatingRepo = new();

    private OutlierDetectionService CreateSut() => new(
        _wineRepo.Object,
        _wineResultRepo.Object,
        _wineRatingRepo.Object);

    // ── RequiresReJudgingAsync ────────────────────────────────────

    [Fact]
    public async Task RequiresReJudgingAsync_WineNotFound_ReturnsFalse()
    {
        _wineResultRepo.Setup(r => r.GetWineResultByWineIdAsync("missing")).ReturnsAsync((WineResult?)null);

        var result = await CreateSut().RequiresReJudgingAsync("missing", 4.0m);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RequiresReJudgingAsync_SpreadAboveThreshold_ReturnsTrue()
    {
        var wineResult = new WineResult { WineId = "wine-1", Spread = 5.0m };
        _wineResultRepo.Setup(r => r.GetWineResultByWineIdAsync("wine-1")).ReturnsAsync(wineResult);

        var result = await CreateSut().RequiresReJudgingAsync("wine-1", 4.0m);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RequiresReJudgingAsync_SpreadAtThreshold_ReturnsFalse()
    {
        var wineResult = new WineResult { WineId = "wine-1", Spread = 4.0m };
        _wineResultRepo.Setup(r => r.GetWineResultByWineIdAsync("wine-1")).ReturnsAsync(wineResult);

        // Spread must be strictly greater than the threshold
        var result = await CreateSut().RequiresReJudgingAsync("wine-1", 4.0m);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RequiresReJudgingAsync_SpreadBelowThreshold_ReturnsFalse()
    {
        var wineResult = new WineResult { WineId = "wine-1", Spread = 3.0m };
        _wineResultRepo.Setup(r => r.GetWineResultByWineIdAsync("wine-1")).ReturnsAsync(wineResult);

        var result = await CreateSut().RequiresReJudgingAsync("wine-1", 4.0m);

        result.Should().BeFalse();
    }

    // ── GetOutlierWinesAsync ──────────────────────────────────────

    [Fact]
    public async Task GetOutlierWinesAsync_NoOutliers_ReturnsEmpty()
    {
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([]);
        _wineResultRepo.Setup(r => r.GetOutlierWineResultsAsync()).ReturnsAsync([]);

        var result = await CreateSut().GetOutlierWinesAsync("event-1");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOutlierWinesAsync_OutlierWineInEvent_ReturnsIt()
    {
        var wine = new Wine { WineId = "wine-1", EventId = "event-1" };
        var wineResult = new WineResult { WineId = "wine-1", IsOutlier = true, Spread = 5.0m };

        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine]);
        _wineResultRepo.Setup(r => r.GetOutlierWineResultsAsync()).ReturnsAsync([wineResult]);

        var result = await CreateSut().GetOutlierWinesAsync("event-1");

        result.Should().HaveCount(1);
        result[0].wine.WineId.Should().Be("wine-1");
        result[0].spread.Should().Be(5.0m);
    }

    [Fact]
    public async Task GetOutlierWinesAsync_OutlierWineFromDifferentEvent_NotIncluded()
    {
        var wine = new Wine { WineId = "wine-1", EventId = "other-event" };
        var wineResult = new WineResult { WineId = "wine-1", IsOutlier = true, Spread = 5.0m };

        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine]);
        _wineResultRepo.Setup(r => r.GetOutlierWineResultsAsync()).ReturnsAsync([wineResult]);

        var result = await CreateSut().GetOutlierWinesAsync("event-1");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOutlierWinesAsync_ResultsOrderedBySpreadDescending()
    {
        var wine1 = new Wine { WineId = "wine-1", EventId = "event-1" };
        var wine2 = new Wine { WineId = "wine-2", EventId = "event-1" };
        var result1 = new WineResult { WineId = "wine-1", IsOutlier = true, Spread = 3.0m };
        var result2 = new WineResult { WineId = "wine-2", IsOutlier = true, Spread = 6.0m };

        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine1, wine2]);
        _wineResultRepo.Setup(r => r.GetOutlierWineResultsAsync()).ReturnsAsync([result1, result2]);

        var outliers = await CreateSut().GetOutlierWinesAsync("event-1");

        outliers.Should().HaveCount(2);
        outliers[0].wine.WineId.Should().Be("wine-2"); // higher spread first
        outliers[1].wine.WineId.Should().Be("wine-1");
    }

    // ── AnalyzeJudgePatternsAsync ─────────────────────────────────

    [Fact]
    public async Task AnalyzeJudgePatternsAsync_NormalJudge_ReturnsNoIssues()
    {
        var wine = new Wine { WineId = "wine-1", EventId = "event-1" };
        // Totals: 11, 14, 16, 19 → avg = 15 (8–16), variance = 8.5 (≥ 2), defect rate = 0
        var ratings = new List<WineRating>
        {
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.0m, Nose = 3.0m, Taste = 6.0m  }, // 11
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.0m, Nose = 3.0m, Taste = 9.0m  }, // 14
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.0m, Nose = 3.0m, Taste = 11.0m }, // 16
            new() { WineId = "wine-1", JudgeId = "judge-1", Appearance = 2.5m, Nose = 3.5m, Taste = 13.0m }, // 19
        };

        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine]);
        _wineRatingRepo.Setup(r => r.GetAllWineRatingsAsync()).ReturnsAsync(ratings);

        var patterns = await CreateSut().AnalyzeJudgePatternsAsync("event-1");

        patterns.Should().NotContainKey("judge-1");
    }

    [Fact]
    public async Task AnalyzeJudgePatternsAsync_JudgeWithVeryLowAverage_FlagsIssue()
    {
        var wine = new Wine { WineId = "wine-1", EventId = "event-1" };
        // Total = 2.0 + 2.0 + 2.0 = 6 → avg < 8 → flagged
        var ratings = Enumerable.Range(1, 5).Select(i => new WineRating
        {
            WineId = "wine-1",
            JudgeId = "harsh-judge",
            Appearance = 0.5m,
            Nose = 0.5m,
            Taste = 2.0m  // Total = 3
        }).ToList();

        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine]);
        _wineRatingRepo.Setup(r => r.GetAllWineRatingsAsync()).ReturnsAsync(ratings);

        var patterns = await CreateSut().AnalyzeJudgePatternsAsync("event-1");

        patterns.Should().ContainKey("harsh-judge");
        patterns["harsh-judge"].Should().ContainSingle(s => s.Contains("Lavt gjennomsnitt"));
    }

    [Fact]
    public async Task AnalyzeJudgePatternsAsync_JudgeWithVeryHighAverage_FlagsIssue()
    {
        var wine = new Wine { WineId = "wine-1", EventId = "event-1" };
        // Total = 3.0 + 4.0 + 13.0 = 20 → avg > 16 → flagged
        var ratings = Enumerable.Range(1, 5).Select(i => new WineRating
        {
            WineId = "wine-1",
            JudgeId = "generous-judge",
            Appearance = 3.0m,
            Nose = 4.0m,
            Taste = 13.0m // Total = 20 (max)
        }).ToList();

        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine]);
        _wineRatingRepo.Setup(r => r.GetAllWineRatingsAsync()).ReturnsAsync(ratings);

        var patterns = await CreateSut().AnalyzeJudgePatternsAsync("event-1");

        patterns.Should().ContainKey("generous-judge");
        patterns["generous-judge"].Should().ContainSingle(s => s.Contains("Høyt gjennomsnitt"));
    }

    [Fact]
    public async Task AnalyzeJudgePatternsAsync_JudgeWithLowVariance_FlagsIssue()
    {
        var wine = new Wine { WineId = "wine-1", EventId = "event-1" };
        // All same scores → variance = 0 < 2 → flagged
        var ratings = Enumerable.Range(1, 6).Select(i => new WineRating
        {
            WineId = "wine-1",
            JudgeId = "monotone-judge",
            Appearance = 2.0m,
            Nose = 3.0m,
            Taste = 8.0m // Total = 13 — average range, but no variance
        }).ToList();

        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([wine]);
        _wineRatingRepo.Setup(r => r.GetAllWineRatingsAsync()).ReturnsAsync(ratings);

        var patterns = await CreateSut().AnalyzeJudgePatternsAsync("event-1");

        patterns.Should().ContainKey("monotone-judge");
        patterns["monotone-judge"].Should().ContainSingle(s => s.Contains("variasjon"));
    }

    [Fact]
    public async Task AnalyzeJudgePatternsAsync_JudgeWithHighDefectRate_FlagsIssue()
    {
        var wines = Enumerable.Range(1, 10).Select(i => new Wine
        {
            WineId = $"wine-{i}",
            EventId = "event-1"
        }).ToList();

        // 4 out of 10 ratings have appearance = 0 → 40% defect rate > 30%
        var ratings = wines.Select((w, i) => new WineRating
        {
            WineId = w.WineId,
            JudgeId = "picky-judge",
            Appearance = i < 4 ? 0m : 2.0m,   // first 4 marked defective
            Nose = 3.0m,
            Taste = 9.0m
        }).ToList();

        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);
        _wineRatingRepo.Setup(r => r.GetAllWineRatingsAsync()).ReturnsAsync(ratings);

        var patterns = await CreateSut().AnalyzeJudgePatternsAsync("event-1");

        patterns.Should().ContainKey("picky-judge");
        patterns["picky-judge"].Should().ContainSingle(s => s.Contains("feilbeheftede"));
    }

    [Fact]
    public async Task AnalyzeJudgePatternsAsync_NoRatingsForEvent_ReturnsEmptyPatterns()
    {
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([]);
        _wineRatingRepo.Setup(r => r.GetAllWineRatingsAsync()).ReturnsAsync([]);

        var patterns = await CreateSut().AnalyzeJudgePatternsAsync("event-1");

        patterns.Should().BeEmpty();
    }
}
