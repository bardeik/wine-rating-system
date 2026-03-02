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

        result.ShouldBe(Classification.NotApproved);
    }

    [Fact]
    public void ClassifyWine_DoesNotMeetGateValues_ReturnsNotApproved()
    {
        var result = _sut.ClassifyWine(18m, 3m, 4m, 11m, isDefective: false, meetsGateValues: false, DefaultEvent());

        result.ShouldBe(Classification.NotApproved);
    }

    [Fact]
    public void ClassifyWine_ScoreAtGoldThreshold_ReturnsGold()
    {
        var result = _sut.ClassifyWine(17.0m, 2.0m, 2.5m, 12.5m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.ShouldBe(Classification.Gold);
    }

    [Fact]
    public void ClassifyWine_ScoreAboveGoldThreshold_ReturnsGold()
    {
        var result = _sut.ClassifyWine(19.5m, 3.0m, 4.0m, 12.5m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.ShouldBe(Classification.Gold);
    }

    [Fact]
    public void ClassifyWine_ScoreAtSilverThreshold_ReturnsSilver()
    {
        var result = _sut.ClassifyWine(15.5m, 2.0m, 2.0m, 11.5m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.ShouldBe(Classification.Silver);
    }

    [Fact]
    public void ClassifyWine_ScoreAtBronzeThreshold_ReturnsBronze()
    {
        var result = _sut.ClassifyWine(14.0m, 2.0m, 2.0m, 10.0m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.ShouldBe(Classification.Bronze);
    }

    [Fact]
    public void ClassifyWine_ScoreAtSpecialMeritThreshold_ReturnsSpecialMerit()
    {
        var result = _sut.ClassifyWine(12.0m, 1.8m, 1.8m, 8.4m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.ShouldBe(Classification.SpecialMerit);
    }

    [Fact]
    public void ClassifyWine_ScoreBelowSpecialMerit_ReturnsAcceptable()
    {
        var result = _sut.ClassifyWine(10.0m, 1.9m, 2.0m, 6.1m, isDefective: false, meetsGateValues: true, DefaultEvent());

        result.ShouldBe(Classification.Acceptable);
    }

    [Fact]
    public void ClassifyWine_UseAdjustedThresholds_UsesAdjustedValues()
    {
        var eventConfig = DefaultEvent();
        eventConfig.UseAdjustedThresholds = true;

        // Score 15.5 is Silver in normal, but Gold in adjusted (Gold adjusted = 15.0)
        var result = _sut.ClassifyWine(15.5m, 2.0m, 2.0m, 11.5m, isDefective: false, meetsGateValues: true, eventConfig);

        result.ShouldBe(Classification.Gold);
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

        result.ShouldBe((decimal)expected);
    }

    [Fact]
    public void GetThreshold_UnknownClassification_ReturnsZero()
    {
        var result = _sut.GetThreshold("UnknownClassification", DefaultEvent());

        result.ShouldBe(0);
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

        _sut.ShouldUseAdjustedThresholds(results).ShouldBeTrue();
    }

    [Fact]
    public void ShouldUseAdjustedThresholds_HasGoldWine_ReturnsFalse()
    {
        var results = new List<WineResult>
        {
            new() { Classification = Classification.Gold },
            new() { Classification = Classification.Silver },
        };

        _sut.ShouldUseAdjustedThresholds(results).ShouldBeFalse();
    }

    [Fact]
    public void ShouldUseAdjustedThresholds_EmptyList_ReturnsTrue()
    {
        _sut.ShouldUseAdjustedThresholds([]).ShouldBeTrue();
    }

    // ── Endrede grenseverdier (admin-konfigurasjon) ────────────────

    [Fact]
    public void ClassifyWine_RaisedGoldThreshold_PreviousGoldScoreDropsToSilver()
    {
        // Score 17.0 er Gull med standardgrense (17.0), men blir Sølv når grensen heves til 18.0
        var eventConfig = DefaultEvent();
        eventConfig.GoldThreshold = 18.0m;

        var result = _sut.ClassifyWine(17.0m, 2.0m, 2.5m, 12.5m, isDefective: false, meetsGateValues: true, eventConfig);

        result.ShouldBe(Classification.Silver);
    }

    [Fact]
    public void ClassifyWine_LoweredGoldThreshold_PreviousSilverScoreBecomesGold()
    {
        // Score 15.5 er Sølv med standardgrense (17.0), men blir Gull når grensen senkes til 15.0
        var eventConfig = DefaultEvent();
        eventConfig.GoldThreshold = 15.0m;

        var result = _sut.ClassifyWine(15.5m, 2.0m, 2.0m, 11.5m, isDefective: false, meetsGateValues: true, eventConfig);

        result.ShouldBe(Classification.Gold);
    }

    [Fact]
    public void ClassifyWine_LoweredBronzeThreshold_PreviousSpecialMeritScoreBecomeBronze()
    {
        // Score 12.5 er Særlig med standardgrense (14.0), men blir Bronse når grensen senkes til 12.0
        var eventConfig = DefaultEvent();
        eventConfig.BronzeThreshold = 12.0m;

        var result = _sut.ClassifyWine(12.5m, 2.0m, 2.0m, 8.5m, isDefective: false, meetsGateValues: true, eventConfig);

        result.ShouldBe(Classification.Bronze);
    }

    [Fact]
    public void ClassifyWine_RaisedSpecialMeritThreshold_PreviousSpecialMeritBecomesAcceptable()
    {
        // Score 12.0 er Særlig med standardgrense (12.0), men blir Akseptabel når grensen heves til 13.0
        var eventConfig = DefaultEvent();
        eventConfig.SpecialMeritThreshold = 13.0m;

        var result = _sut.ClassifyWine(12.0m, 1.9m, 2.0m, 8.1m, isDefective: false, meetsGateValues: true, eventConfig);

        result.ShouldBe(Classification.Acceptable);
    }

    [Fact]
    public void ClassifyWine_LoweredAppearanceGateValue_PreviouslyRejectedWineNowApproved()
    {
        // Appearance 1.5 < standardgate 1.8 → ikke godkjent. Gate senkes til 1.4 → godkjent (Bronse)
        var eventConfig = DefaultEvent();
        eventConfig.AppearanceGateValue = 1.4m;
        eventConfig.BronzeThreshold = 14.0m;

        var result = _sut.ClassifyWine(14.0m, 1.5m, 2.0m, 10.5m, isDefective: false, meetsGateValues: true, eventConfig);

        result.ShouldBe(Classification.Bronze);
    }

    [Fact]
    public void ClassifyWine_RaisedTasteGateValue_PreviouslyApprovedWineNowRejected()
    {
        // meetsGateValues=false simulerer at hevet gate-verdi fører til underkjenning
        var eventConfig = DefaultEvent();
        eventConfig.TasteGateValue = 9.0m;

        var result = _sut.ClassifyWine(14.0m, 2.0m, 2.0m, 8.0m, isDefective: false, meetsGateValues: false, eventConfig);

        result.ShouldBe(Classification.NotApproved);
    }

    [Fact]
    public void ClassifyWine_CustomAdjustedThresholds_UsesNewAdjustedValues()
    {
        // Nedjustert Gull settes til 14.0 (lavere enn standard 15.0); score 14.5 skal gi Gull
        var eventConfig = DefaultEvent();
        eventConfig.UseAdjustedThresholds = true;
        eventConfig.AdjustedGoldThreshold = 14.0m;

        var result = _sut.ClassifyWine(14.5m, 2.0m, 2.0m, 10.5m, isDefective: false, meetsGateValues: true, eventConfig);

        result.ShouldBe(Classification.Gold);
    }

    [Theory]
    [InlineData(16.0, 18.0, Classification.Silver)]   // hevet Gull gir Sølv
    [InlineData(16.0, 15.5, Classification.Gold)]     // senket Gull gir Gull
    [InlineData(13.5, 14.0, Classification.SpecialMerit)] // score under Bronse gir Særlig
    [InlineData(13.5, 13.0, Classification.Gold)]     // senket Gull under scoren gir Gull
    public void ClassifyWine_VariousCustomGoldThresholds_ReturnsExpectedClassification(
        double score, double goldThreshold, string expected)
    {
        var eventConfig = DefaultEvent();
        eventConfig.GoldThreshold = (decimal)goldThreshold;

        var result = _sut.ClassifyWine((decimal)score, 2.0m, 2.5m, (decimal)score - 4.5m,
            isDefective: false, meetsGateValues: true, eventConfig);

        result.ShouldBe(expected);
    }

    [Fact]
    public void GetThreshold_AfterAdminChangesGoldThreshold_ReturnsNewValue()
    {
        var eventConfig = DefaultEvent();
        eventConfig.GoldThreshold = 18.5m;

        _sut.GetThreshold(Classification.Gold, eventConfig).ShouldBe(18.5m);
    }

    [Fact]
    public void GetThreshold_AfterAdminChangesAdjustedSilverThreshold_ReturnsNewValueWhenAdjustedMode()
    {
        var eventConfig = DefaultEvent();
        eventConfig.UseAdjustedThresholds = true;
        eventConfig.AdjustedSilverThreshold = 13.0m;

        _sut.GetThreshold(Classification.Silver, eventConfig).ShouldBe(13.0m);
    }
}
