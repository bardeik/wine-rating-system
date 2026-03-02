using WineApp.Models;
using WineApp.Services;

namespace WineApp.Tests.Services;

public class WineValidationServiceTests
{
    // Use the real system clock for tests that don’t care about time.
    // Use FrozenTimeProvider in tests that verify time-sensitive logic.
    private readonly WineValidationService _sut = new(TimeProvider.System);

    // ── ValidateGrapeBlend ─────────────────────────────────────────

    [Fact]
    public void ValidateGrapeBlend_NullDictionary_ReturnsFalseWithMessage()
    {
        var (isValid, message) = _sut.ValidateGrapeBlend(null!);

        isValid.ShouldBeFalse();
        message.ShouldNotBeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_EmptyDictionary_ReturnsFalseWithMessage()
    {
        var (isValid, message) = _sut.ValidateGrapeBlend([]);

        isValid.ShouldBeFalse();
        message.ShouldNotBeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_SumNot100_ReturnsFalseWithMessage()
    {
        var blend = new Dictionary<string, decimal> { ["Riesling"] = 60m, ["Chardonnay"] = 30m }; // sum = 90

        var (isValid, message) = _sut.ValidateGrapeBlend(blend);

        isValid.ShouldBeFalse();
        message.ShouldContain("100");
    }

    [Fact]
    public void ValidateGrapeBlend_NegativeValue_ReturnsFalseWithMessage()
    {
        var blend = new Dictionary<string, decimal> { ["Riesling"] = 110m, ["Chardonnay"] = -10m }; // sum = 100 but negative

        var (isValid, message) = _sut.ValidateGrapeBlend(blend);

        isValid.ShouldBeFalse();
        message.ShouldNotBeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_ZeroValue_ReturnsFalseWithMessage()
    {
        var blend = new Dictionary<string, decimal> { ["Riesling"] = 100m, ["Chardonnay"] = 0m };

        var (isValid, message) = _sut.ValidateGrapeBlend(blend);

        isValid.ShouldBeFalse();
        message.ShouldNotBeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_ValidBlend_ReturnsTrue()
    {
        var blend = new Dictionary<string, decimal> { ["Riesling"] = 60m, ["Chardonnay"] = 40m };

        var (isValid, message) = _sut.ValidateGrapeBlend(blend);

        isValid.ShouldBeTrue();
        message.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_ValidSingleGrape_ReturnsTrue()
    {
        var blend = new Dictionary<string, decimal> { ["Solaris"] = 100m };

        var (isValid, _) = _sut.ValidateGrapeBlend(blend);

        isValid.ShouldBeTrue();
    }

    [Fact]
    public void ValidateGrapeBlend_SumWithinTolerance_ReturnsTrue()
    {
        // 99.99 is within 0.01 tolerance
        var blend = new Dictionary<string, decimal> { ["Solaris"] = 99.99m, ["Rondo"] = 0.01m };

        var (isValid, _) = _sut.ValidateGrapeBlend(blend);

        isValid.ShouldBeTrue();
    }

    // ── ValidateVinbondeEligibility ────────────────────────────────

    [Fact]
    public void ValidateVinbondeEligibility_NotVinbonde_ReturnsTrue()
    {
        var wine = new Wine { IsVinbonde = false, Group = WineGroup.B };

        var (isValid, message) = _sut.ValidateVinbondeEligibility(wine);

        isValid.ShouldBeTrue();
        message.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateVinbondeEligibility_VinbondeGroupA1_ReturnsTrue()
    {
        var wine = new Wine { IsVinbonde = true, Group = WineGroup.A1 };

        var (isValid, message) = _sut.ValidateVinbondeEligibility(wine);

        isValid.ShouldBeTrue();
        message.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(WineGroup.A2)]
    [InlineData(WineGroup.B)]
    [InlineData(WineGroup.C)]
    [InlineData(WineGroup.D)]
    public void ValidateVinbondeEligibility_VinbondeNonA1Group_ReturnsFalse(WineGroup group)
    {
        var wine = new Wine { IsVinbonde = true, Group = group };

        var (isValid, message) = _sut.ValidateVinbondeEligibility(wine);

        isValid.ShouldBeFalse();
        message.ShouldNotBeEmpty();
    }

    // ── ValidateWineRegistration ──────────────────────────────────

    [Fact]
    public void ValidateWineRegistration_ValidWine_ReturnsNoErrors()
    {
        var wine = ValidWine();

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeTrue();
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateWineRegistration_EmptyName_ReturnsError()
    {
        var wine = ValidWine();
        wine.Name = string.Empty;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.Count(e => e.Contains("Vinnavn")).ShouldBe(1);
    }

    [Fact]
    public void ValidateWineRegistration_EmptyRatingName_ReturnsError()
    {
        var wine = ValidWine();
        wine.RatingName = string.Empty;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.Count(e => e.Contains("Vurderingsnavn")).ShouldBe(1);
    }

    [Fact]
    public void ValidateWineRegistration_VintageTooOld_ReturnsError()
    {
        var wine = ValidWine();
        wine.Vintage = 1800; // always below minimum of 1900

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.Count(e => e.Contains("Årgang")).ShouldBe(1);
    }

    [Fact]
    public void ValidateWineRegistration_VintageTooFarInFuture_ReturnsError()
    {
        // Freeze clock at 2026-06-01 → max allowed vintage is 2027
        var clock = FrozenTimeProvider.At(2026, 6, 1);
        var sut = new WineValidationService(clock);
        var wine = ValidWine();
        wine.Vintage = 2028; // one year beyond the allowed maximum

        var (isValid, errors) = sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.Count(e => e.Contains("Årgang") && e.Contains("2027")).ShouldBe(1);
    }

    [Fact]
    public void ValidateWineRegistration_VintageAtMaxAllowed_IsValid()
    {
        // Freeze clock at 2026-06-01 → max allowed vintage is exactly 2027
        var clock = FrozenTimeProvider.At(2026, 6, 1);
        var sut = new WineValidationService(clock);
        var wine = ValidWine();
        wine.Vintage = 2027;

        var (isValid, errors) = sut.ValidateWineRegistration(wine);

        isValid.ShouldBeTrue();
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateWineRegistration_NegativeAlcohol_ReturnsError()
    {
        var wine = ValidWine();
        wine.AlcoholPercentage = -1m;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.Count(e => e.Contains("Alkohol")).ShouldBe(1);
    }

    [Fact]
    public void ValidateWineRegistration_EmptyCountry_ReturnsError()
    {
        var wine = ValidWine();
        wine.Country = string.Empty;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.Count(e => e.Contains("Land")).ShouldBe(1);
    }

    [Fact]
    public void ValidateWineRegistration_EmptyProducerId_ReturnsError()
    {
        var wine = ValidWine();
        wine.WineProducerId = string.Empty;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.Count(e => e.Contains("Vinprodusent")).ShouldBe(1);
    }

    [Fact]
    public void ValidateWineRegistration_GroupA2FromNorway_ReturnsError()
    {
        var wine = ValidWine();
        wine.Group = WineGroup.A2;
        wine.Country = "Norge";

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.Count(e => e.Contains("A2")).ShouldBe(1);
    }

    [Fact]
    public void ValidateWineRegistration_InvalidGrapeBlend_ReturnsError()
    {
        var wine = ValidWine();
        wine.GrapeBlend = new Dictionary<string, decimal> { ["Solaris"] = 50m }; // sum != 100

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void ValidateWineRegistration_VinbondeWithWrongGroup_ReturnsError()
    {
        var wine = ValidWine();
        wine.IsVinbonde = true;
        wine.Group = WineGroup.B;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.ShouldBeFalse();
        errors.Count(e => e.Contains("Vinbonde")).ShouldBe(1);
    }

    // ── ValidateWineForRating ─────────────────────────────────────

    [Fact]
    public void ValidateWineForRating_Unpaid_ReturnsFalse()
    {
        var wine = new Wine { IsPaid = false, WineNumber = 5 };

        var (isValid, message) = _sut.ValidateWineForRating(wine);

        isValid.ShouldBeFalse();
        message.ShouldNotBeEmpty();
    }

    [Fact]
    public void ValidateWineForRating_PaidButNoWineNumber_ReturnsFalse()
    {
        var wine = new Wine { IsPaid = true, WineNumber = null };

        var (isValid, message) = _sut.ValidateWineForRating(wine);

        isValid.ShouldBeFalse();
        message.ShouldNotBeEmpty();
    }

    [Fact]
    public void ValidateWineForRating_PaidWithWineNumber_ReturnsTrue()
    {
        var wine = new Wine { IsPaid = true, WineNumber = 3 };

        var (isValid, message) = _sut.ValidateWineForRating(wine);

        isValid.ShouldBeTrue();
        message.ShouldBeEmpty();
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static Wine ValidWine() => new()
    {
        Name = "Testvin",
        RatingName = "Test Vurdering",
        Vintage = 2022,
        AlcoholPercentage = 12.5m,
        Country = "Norge",
        Group = WineGroup.A1,
        WineProducerId = "producer-1",
        GrapeBlend = new Dictionary<string, decimal> { ["Solaris"] = 100m }
    };
}
