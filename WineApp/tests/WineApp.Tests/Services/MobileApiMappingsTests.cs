using System.Text;

namespace WineApp.Tests.Services;

/// <summary>
/// Unit tests for the pure mapping helpers and Base64Url utilities in
/// <see cref="MobileApiMappings"/>. These are internal helpers extracted so
/// that the threshold-selection business logic can be verified independently.
/// </summary>
public class MobileApiMappingsTests
{
    // ── MapEvent — standard thresholds ───────────────────────────

    [Fact]
    public void MapEvent_UseAdjustedThresholdsFalse_UsesStandardThresholds()
    {
        var evt = MakeEvent(useAdjusted: false);

        var dto = MobileApiMappings.MapEvent(evt);

        dto.GoldThreshold.ShouldBe(evt.GoldThreshold);
        dto.SilverThreshold.ShouldBe(evt.SilverThreshold);
        dto.BronzeThreshold.ShouldBe(evt.BronzeThreshold);
        dto.SpecialMeritThreshold.ShouldBe(evt.SpecialMeritThreshold);
    }

    [Fact]
    public void MapEvent_UseAdjustedThresholdsTrue_UsesAdjustedThresholds()
    {
        var evt = MakeEvent(useAdjusted: true);

        var dto = MobileApiMappings.MapEvent(evt);

        dto.GoldThreshold.ShouldBe(evt.AdjustedGoldThreshold);
        dto.SilverThreshold.ShouldBe(evt.AdjustedSilverThreshold);
        dto.BronzeThreshold.ShouldBe(evt.AdjustedBronzeThreshold);
        dto.SpecialMeritThreshold.ShouldBe(evt.AdjustedSpecialMeritThreshold);
    }

    [Fact]
    public void MapEvent_UseAdjustedThresholdsFalse_DoesNotReturnAdjustedValues()
    {
        var evt = MakeEvent(useAdjusted: false);
        // Confirm standard ≠ adjusted in the fixture so the test is meaningful
        evt.GoldThreshold.ShouldNotBe(evt.AdjustedGoldThreshold);

        var dto = MobileApiMappings.MapEvent(evt);

        dto.GoldThreshold.ShouldNotBe(evt.AdjustedGoldThreshold);
    }

    [Fact]
    public void MapEvent_MapsScalarFieldsCorrectly()
    {
        var evt = MakeEvent(useAdjusted: false);

        var dto = MobileApiMappings.MapEvent(evt);

        dto.EventId.ShouldBe(evt.EventId);
        dto.Name.ShouldBe(evt.Name);
        dto.Year.ShouldBe(evt.Year);
        dto.AppearanceGateValue.ShouldBe(evt.AppearanceGateValue);
        dto.NoseGateValue.ShouldBe(evt.NoseGateValue);
        dto.TasteGateValue.ShouldBe(evt.TasteGateValue);
        dto.IsActive.ShouldBe(evt.IsActive);
    }

    // ── MapWine ───────────────────────────────────────────────────

    [Fact]
    public void MapWine_MapsAllPropertiesCorrectly()
    {
        var wine = new Wine
        {
            WineId          = "wine-1",
            WineNumber      = 42,
            Name            = "Riesling Spätlese",
            RatingName      = "Vin A",
            Vintage         = 2020,
            AlcoholPercentage = 12.5m,
            Country         = "Germany",
            Group           = WineGroup.A1,
            Class           = WineClass.Unge,
            Category        = WineCategory.Hvitvin,
            WineProducerId  = "prod-1",
            EventId         = "event-1",
            IsPaid          = true,
        };

        var dto = MobileApiMappings.MapWine(wine);

        dto.WineId.ShouldBe("wine-1");
        dto.WineNumber.ShouldBe(42);
        dto.Name.ShouldBe("Riesling Spätlese");
        dto.RatingName.ShouldBe("Vin A");
        dto.Vintage.ShouldBe(2020);
        dto.AlcoholPercentage.ShouldBe(12.5m);
        dto.Country.ShouldBe("Germany");
        dto.Group.ShouldBe(WineGroup.A1.ToString());
        dto.Class.ShouldBe(WineClass.Unge.ToString());
        dto.Category.ShouldBe(WineCategory.Hvitvin.ToString());
        dto.WineProducerId.ShouldBe("prod-1");
        dto.EventId.ShouldBe("event-1");
        dto.IsPaid.ShouldBeTrue();
    }

    [Fact]
    public void MapWine_NullEventId_MapsToNull()
    {
        var wine = new Wine { EventId = null };

        var dto = MobileApiMappings.MapWine(wine);

        dto.EventId.ShouldBeNull();
    }

    [Fact]
    public void MapWine_NullWineNumber_MapsToNull()
    {
        var wine = new Wine { WineNumber = null };

        var dto = MobileApiMappings.MapWine(wine);

        dto.WineNumber.ShouldBeNull();
    }

    [Fact]
    public void MapWine_EnumValuesConvertedToStrings()
    {
        var wine = new Wine
        {
            Group    = WineGroup.B,
            Class    = WineClass.Eldre,
            Category = WineCategory.Rodvin,
        };

        var dto = MobileApiMappings.MapWine(wine);

        dto.Group.ShouldBe("B");
        dto.Class.ShouldBe("Eldre");
        dto.Category.ShouldBe("Rodvin");
    }

    // ── MapRating ─────────────────────────────────────────────────

    [Fact]
    public void MapRating_MapsAllPropertiesCorrectly()
    {
        var rating = new WineRating
        {
            WineRatingId = "rating-1",
            Appearance   = 2.0m,
            Nose         = 3.5m,
            Taste        = 10.0m,
            Comment      = "Excellent",
            JudgeId      = "judge-1",
            WineId       = "wine-1",
        };

        var dto = MobileApiMappings.MapRating(rating);

        dto.WineRatingId.ShouldBe("rating-1");
        dto.Appearance.ShouldBe(2.0m);
        dto.Nose.ShouldBe(3.5m);
        dto.Taste.ShouldBe(10.0m);
        dto.Comment.ShouldBe("Excellent");
        dto.JudgeId.ShouldBe("judge-1");
        dto.WineId.ShouldBe("wine-1");
    }

    [Fact]
    public void MapRating_TotalIsSumOfScores()
    {
        var rating = new WineRating
        {
            Appearance = 2.0m,
            Nose       = 3.0m,
            Taste      = 9.0m,
        };

        var dto = MobileApiMappings.MapRating(rating);

        dto.Total.ShouldBe(14.0m);
    }

    [Fact]
    public void MapRating_EmptyComment_MapsToEmptyString()
    {
        var rating = new WineRating { Comment = string.Empty };

        var dto = MobileApiMappings.MapRating(rating);

        dto.Comment.ShouldBe(string.Empty);
    }

    // ── Base64UrlEncode / Base64UrlDecode ─────────────────────────

    [Fact]
    public void Base64UrlEncode_Roundtrip_ReturnsOriginalBytes()
    {
        var original = Encoding.UTF8.GetBytes("hello world");

        var encoded = MobileApiMappings.Base64UrlEncode(original);
        var decoded = MobileApiMappings.Base64UrlDecode(encoded);

        decoded.ShouldBe(original);
    }

    [Fact]
    public void Base64UrlEncode_OutputContainsNoStandardBase64PaddingOrSpecialChars()
    {
        var input = Encoding.UTF8.GetBytes("test-payload-that-needs-padding");

        var encoded = MobileApiMappings.Base64UrlEncode(input);

        encoded.ShouldNotContain("+");
        encoded.ShouldNotContain("/");
        encoded.ShouldNotContain("=");
    }

    [Fact]
    public void Base64UrlEncode_OutputUsesUrlSafeChars()
    {
        // Force a payload that would contain + or / in standard Base64.
        // 0xFB, 0xFF produces ++ or similar characters in standard base64.
        var input = new byte[] { 0xFB, 0xFF, 0xFE };

        var encoded = MobileApiMappings.Base64UrlEncode(input);

        encoded.ShouldNotContain("+");
        encoded.ShouldNotContain("/");
    }

    [Fact]
    public void Base64UrlDecode_Roundtrip_CorrectlyHandlesPaddingVariants()
    {
        // Test several lengths to exercise 0, 1, 2, 3 padding characters.
        var inputs = new[]
        {
            "a",
            "ab",
            "abc",
            "abcd",
            "Hello, World!",
        };

        foreach (var s in inputs)
        {
            var bytes   = Encoding.UTF8.GetBytes(s);
            var encoded = MobileApiMappings.Base64UrlEncode(bytes);
            var decoded = MobileApiMappings.Base64UrlDecode(encoded);

            Encoding.UTF8.GetString(decoded).ShouldBe(s);
        }
    }

    [Fact]
    public void Base64UrlEncode_EmptyInput_ReturnsEmptyString()
    {
        var encoded = MobileApiMappings.Base64UrlEncode([]);

        encoded.ShouldBe(string.Empty);
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static Event MakeEvent(bool useAdjusted) => new()
    {
        EventId                  = "event-1",
        Name                     = "Norsk Vinfestival",
        Year                     = 2026,
        IsActive                 = true,
        UseAdjustedThresholds    = useAdjusted,
        GoldThreshold            = 17.0m,
        SilverThreshold          = 15.5m,
        BronzeThreshold          = 14.0m,
        SpecialMeritThreshold    = 12.0m,
        AdjustedGoldThreshold    = 15.0m,
        AdjustedSilverThreshold  = 14.0m,
        AdjustedBronzeThreshold  = 13.0m,
        AdjustedSpecialMeritThreshold = 11.5m,
        AppearanceGateValue      = 1.8m,
        NoseGateValue            = 1.8m,
        TasteGateValue           = 5.8m,
    };
}
