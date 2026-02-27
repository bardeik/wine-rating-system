using FluentAssertions;
using WineApp.Models;
using WineApp.Services;

namespace WineApp.Tests.Services;

public class WineValidationServiceTests
{
    private readonly WineValidationService _sut = new();

    // ── ValidateGrapeBlend ─────────────────────────────────────────

    [Fact]
    public void ValidateGrapeBlend_NullDictionary_ReturnsFalseWithMessage()
    {
        var (isValid, message) = _sut.ValidateGrapeBlend(null!);

        isValid.Should().BeFalse();
        message.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_EmptyDictionary_ReturnsFalseWithMessage()
    {
        var (isValid, message) = _sut.ValidateGrapeBlend([]);

        isValid.Should().BeFalse();
        message.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_SumNot100_ReturnsFalseWithMessage()
    {
        var blend = new Dictionary<string, decimal> { ["Riesling"] = 60m, ["Chardonnay"] = 30m }; // sum = 90

        var (isValid, message) = _sut.ValidateGrapeBlend(blend);

        isValid.Should().BeFalse();
        message.Should().Contain("100");
    }

    [Fact]
    public void ValidateGrapeBlend_NegativeValue_ReturnsFalseWithMessage()
    {
        var blend = new Dictionary<string, decimal> { ["Riesling"] = 110m, ["Chardonnay"] = -10m }; // sum = 100 but negative

        var (isValid, message) = _sut.ValidateGrapeBlend(blend);

        isValid.Should().BeFalse();
        message.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_ZeroValue_ReturnsFalseWithMessage()
    {
        var blend = new Dictionary<string, decimal> { ["Riesling"] = 100m, ["Chardonnay"] = 0m };

        var (isValid, message) = _sut.ValidateGrapeBlend(blend);

        isValid.Should().BeFalse();
        message.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_ValidBlend_ReturnsTrue()
    {
        var blend = new Dictionary<string, decimal> { ["Riesling"] = 60m, ["Chardonnay"] = 40m };

        var (isValid, message) = _sut.ValidateGrapeBlend(blend);

        isValid.Should().BeTrue();
        message.Should().BeEmpty();
    }

    [Fact]
    public void ValidateGrapeBlend_ValidSingleGrape_ReturnsTrue()
    {
        var blend = new Dictionary<string, decimal> { ["Solaris"] = 100m };

        var (isValid, _) = _sut.ValidateGrapeBlend(blend);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateGrapeBlend_SumWithinTolerance_ReturnsTrue()
    {
        // 99.99 is within 0.01 tolerance
        var blend = new Dictionary<string, decimal> { ["Solaris"] = 99.99m, ["Rondo"] = 0.01m };

        var (isValid, _) = _sut.ValidateGrapeBlend(blend);

        isValid.Should().BeTrue();
    }

    // ── ValidateVinbondeEligibility ────────────────────────────────

    [Fact]
    public void ValidateVinbondeEligibility_NotVinbonde_ReturnsTrue()
    {
        var wine = new Wine { IsVinbonde = false, Group = WineGroup.B };

        var (isValid, message) = _sut.ValidateVinbondeEligibility(wine);

        isValid.Should().BeTrue();
        message.Should().BeEmpty();
    }

    [Fact]
    public void ValidateVinbondeEligibility_VinbondeGroupA1_ReturnsTrue()
    {
        var wine = new Wine { IsVinbonde = true, Group = WineGroup.A1 };

        var (isValid, message) = _sut.ValidateVinbondeEligibility(wine);

        isValid.Should().BeTrue();
        message.Should().BeEmpty();
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

        isValid.Should().BeFalse();
        message.Should().NotBeEmpty();
    }

    // ── ValidateWineRegistration ──────────────────────────────────

    [Fact]
    public void ValidateWineRegistration_ValidWine_ReturnsNoErrors()
    {
        var wine = ValidWine();

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateWineRegistration_EmptyName_ReturnsError()
    {
        var wine = ValidWine();
        wine.Name = string.Empty;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Vinnavn"));
    }

    [Fact]
    public void ValidateWineRegistration_EmptyRatingName_ReturnsError()
    {
        var wine = ValidWine();
        wine.RatingName = string.Empty;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Vurderingsnavn"));
    }

    [Fact]
    public void ValidateWineRegistration_InvalidVintage_ReturnsError()
    {
        var wine = ValidWine();
        wine.Vintage = 1800;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Årgang"));
    }

    [Fact]
    public void ValidateWineRegistration_NegativeAlcohol_ReturnsError()
    {
        var wine = ValidWine();
        wine.AlcoholPercentage = -1m;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Alkohol"));
    }

    [Fact]
    public void ValidateWineRegistration_EmptyCountry_ReturnsError()
    {
        var wine = ValidWine();
        wine.Country = string.Empty;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Land"));
    }

    [Fact]
    public void ValidateWineRegistration_EmptyProducerId_ReturnsError()
    {
        var wine = ValidWine();
        wine.WineProducerId = string.Empty;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Vinprodusent"));
    }

    [Fact]
    public void ValidateWineRegistration_GroupA2FromNorway_ReturnsError()
    {
        var wine = ValidWine();
        wine.Group = WineGroup.A2;
        wine.Country = "Norge";

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("A2"));
    }

    [Fact]
    public void ValidateWineRegistration_InvalidGrapeBlend_ReturnsError()
    {
        var wine = ValidWine();
        wine.GrapeBlend = new Dictionary<string, decimal> { ["Solaris"] = 50m }; // sum != 100

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateWineRegistration_VinbondeWithWrongGroup_ReturnsError()
    {
        var wine = ValidWine();
        wine.IsVinbonde = true;
        wine.Group = WineGroup.B;

        var (isValid, errors) = _sut.ValidateWineRegistration(wine);

        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Vinbonde"));
    }

    // ── ValidateWineForRating ─────────────────────────────────────

    [Fact]
    public void ValidateWineForRating_Unpaid_ReturnsFalse()
    {
        var wine = new Wine { IsPaid = false, WineNumber = 5 };

        var (isValid, message) = _sut.ValidateWineForRating(wine);

        isValid.Should().BeFalse();
        message.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateWineForRating_PaidButNoWineNumber_ReturnsFalse()
    {
        var wine = new Wine { IsPaid = true, WineNumber = null };

        var (isValid, message) = _sut.ValidateWineForRating(wine);

        isValid.Should().BeFalse();
        message.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateWineForRating_PaidWithWineNumber_ReturnsTrue()
    {
        var wine = new Wine { IsPaid = true, WineNumber = 3 };

        var (isValid, message) = _sut.ValidateWineForRating(wine);

        isValid.Should().BeTrue();
        message.Should().BeEmpty();
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
