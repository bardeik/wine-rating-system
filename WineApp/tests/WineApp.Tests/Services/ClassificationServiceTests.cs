using FluentAssertions;
using WineApp.Models;
using WineApp.Services;

namespace WineApp.Tests.Services;

public class ClassificationServiceTests
{
    private readonly ClassificationService _sut = new();

    private static Event DefaultEvent() => new()
    {
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
        UseAdjustedThresholds = false
    };

    // ── ClassifyWine ──────────────────────────────────────────────

    [Fact]
    public void ClassifyWine_DefectiveWine_ReturnsNotApproved()
    {
        var result = _sut.ClassifyWine(18m, 3m, 4m, 11m, isDefective: true, meetsGateValues: true, DefaultEvent());

        result.Should().Be(Classification.NotApproved);
    }

    [Fact]
    public void ClassifyWine_DoesNotMeetGateValues_ReturnsNotApproved()
    {
        var result = _sut.ClassifyWine(18m, 3m, 4m, 11m, isDefective: false, meetsGateValues: false, DefaultEvent());

        result.Should().Be(Classification.NotApproved);
    }

    [Fact]
    public void ClassifyWine_ScoreAtGoldThreshold_ReturnsGold()
    {
        var result = _sut.ClassifyWine(17.0m, 2.0m, 2.5m, 12.5m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.Should().Be(Classification.Gold);
    }

    [Fact]
    public void ClassifyWine_ScoreAboveGoldThreshold_ReturnsGold()
    {
        var result = _sut.ClassifyWine(19.5m, 3.0m, 4.0m, 12.5m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.Should().Be(Classification.Gold);
    }

    [Fact]
    public void ClassifyWine_ScoreAtSilverThreshold_ReturnsSilver()
    {
        var result = _sut.ClassifyWine(15.5m, 2.0m, 2.0m, 11.5m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.Should().Be(Classification.Silver);
    }

    [Fact]
    public void ClassifyWine_ScoreAtBronzeThreshold_ReturnsBronze()
    {
        var result = _sut.ClassifyWine(14.0m, 2.0m, 2.0m, 10.0m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.Should().Be(Classification.Bronze);
    }

    [Fact]
    public void ClassifyWine_ScoreAtSpecialMeritThreshold_ReturnsSpecialMerit()
    {
        var result = _sut.ClassifyWine(12.0m, 1.8m, 1.8m, 8.4m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.Should().Be(Classification.SpecialMerit);
    }

    [Fact]
    public void ClassifyWine_ScoreBelowSpecialMerit_ReturnsAcceptable()
    {
        var result = _sut.ClassifyWine(10.0m, 1.9m, 2.0m, 6.1m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.Should().Be(Classification.Acceptable);
    }

    [Fact]
    public void ClassifyWine_UseAdjustedThresholds_UsesAdjustedValues()
    {
        var eventConfig = DefaultEvent();
        eventConfig.UseAdjustedThresholds = true;

        // Score 15.5 is Silver in normal, but Gold in adjusted (Gold adjusted = 15.0)
        var result = _sut.ClassifyWine(15.5m, 2.0m, 2.0m, 11.5m, isDefective: false, meetsGateValues: true, eventConfig);

        result.Should().Be(Classification.Gold);
    }

    // ── GetThreshold ──────────────────────────────────────────────

    [Theory]
    [InlineData(Classification.Gold, false, 17.0)]
    [InlineData(Classification.Silver, false, 15.5)]
    [InlineData(Classification.Bronze, false, 14.0)]
    [InlineData(Classification.SpecialMerit, false, 12.0)]
    [InlineData(Classification.Gold, true, 15.0)]
    [InlineData(Classification.Silver, true, 14.0)]
    [InlineData(Classification.Bronze, true, 13.0)]
    [InlineData(Classification.SpecialMerit, true, 11.5)]
    public void GetThreshold_ReturnsCorrectThreshold(string classification, bool useAdjusted, double expected)
    {
        var eventConfig = DefaultEvent();
        eventConfig.UseAdjustedThresholds = useAdjusted;

        var result = _sut.GetThreshold(classification, eventConfig);

        result.Should().Be((decimal)expected);
    }

    [Fact]
    public void GetThreshold_UnknownClassification_ReturnsZero()
    {
        var result = _sut.GetThreshold("UnknownClassification", DefaultEvent());

        result.Should().Be(0);
    }

    // ── ShouldUseAdjustedThresholds ───────────────────────────────

    [Fact]
    public void ShouldUseAdjustedThresholds_NoGoldWines_ReturnsTrue()
    {
        var results = new List<WineResult>
        {
            new() { Classification = Classification.Silver },
            new() { Classification = Classification.Bronze },
        };

        _sut.ShouldUseAdjustedThresholds(results).Should().BeTrue();
    }

    [Fact]
    public void ShouldUseAdjustedThresholds_HasGoldWine_ReturnsFalse()
    {
        var results = new List<WineResult>
        {
            new() { Classification = Classification.Gold },
            new() { Classification = Classification.Silver },
        };

        _sut.ShouldUseAdjustedThresholds(results).Should().BeFalse();
    }

    [Fact]
    public void ShouldUseAdjustedThresholds_EmptyList_ReturnsTrue()
    {
        _sut.ShouldUseAdjustedThresholds([]).Should().BeTrue();
    }
}
