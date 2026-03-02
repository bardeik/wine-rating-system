using Moq;
using WineApp.Data;
using WineApp.Models;
using WineApp.Services;

namespace WineApp.Tests.Services;

public class WineNumberServiceTests
{
    private readonly Mock<IWineRepository> _wineRepo = new();

    private WineNumberService CreateSut() => new(_wineRepo.Object);

    // ── GetCategoryOrder ──────────────────────────────────────────

    [Fact]
    public void GetCategoryOrder_ReturnsSixCategories()
    {
        var order = CreateSut().GetCategoryOrder();

        order.Count.ShouldBe(6);
    }

    [Fact]
    public void GetCategoryOrder_HvitvinIsFirst()
    {
        var order = CreateSut().GetCategoryOrder();

        order[0].ShouldBe(WineCategory.Hvitvin);
    }

    [Fact]
    public void GetCategoryOrder_HetvinIsLast()
    {
        var order = CreateSut().GetCategoryOrder();

        order[^1].ShouldBe(WineCategory.Hetvin);
    }

    [Fact]
    public void GetCategoryOrder_CorrectSequence()
    {
        var expected = new List<WineCategory>
        {
            WineCategory.Hvitvin,
            WineCategory.Rosevin,
            WineCategory.Dessertvin,
            WineCategory.Rodvin,
            WineCategory.Mousserendevin,
            WineCategory.Hetvin
        };

        var order = CreateSut().GetCategoryOrder();

        order.ShouldBe(expected);
    }

    [Fact]
    public void GetCategoryOrder_ContainsAllCategories()
    {
        var order = CreateSut().GetCategoryOrder();

        var allCategories = Enum.GetValues<WineCategory>();
        order.ShouldBe(allCategories, ignoreOrder: true);
    }

    // ── GetNextWineNumberAsync ────────────────────────────────────

    [Fact]
    public async Task GetNextWineNumberAsync_NoWinesWithNumbers_ReturnsOne()
    {
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([]);

        var next = await CreateSut().GetNextWineNumberAsync("event-1");

        next.ShouldBe(1);
    }

    [Fact]
    public async Task GetNextWineNumberAsync_ExistingWines_ReturnsMaxPlusOne()
    {
        var wines = new List<Wine>
        {
            new() { EventId = "event-1", WineNumber = 3 },
            new() { EventId = "event-1", WineNumber = 7 },
            new() { EventId = "event-1", WineNumber = 5 },
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);

        var next = await CreateSut().GetNextWineNumberAsync("event-1");

        next.ShouldBe(8);
    }

    [Fact]
    public async Task GetNextWineNumberAsync_WinesWithoutNumbers_Ignored()
    {
        var wines = new List<Wine>
        {
            new() { EventId = "event-1", WineNumber = 2 },
            new() { EventId = "event-1", WineNumber = null }, // no number assigned yet
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);

        var next = await CreateSut().GetNextWineNumberAsync("event-1");

        next.ShouldBe(3);
    }

    // ── ValidateWineNumbersAsync ──────────────────────────────────

    [Fact]
    public async Task ValidateWineNumbersAsync_NoNumberedWines_ReturnsTrue()
    {
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync([]);

        var result = await CreateSut().ValidateWineNumbersAsync("event-1");

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateWineNumbersAsync_UniqueNumbers_ReturnsTrue()
    {
        var wines = new List<Wine>
        {
            new() { EventId = "event-1", WineNumber = 1 },
            new() { EventId = "event-1", WineNumber = 2 },
            new() { EventId = "event-1", WineNumber = 3 },
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);

        var result = await CreateSut().ValidateWineNumbersAsync("event-1");

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateWineNumbersAsync_DuplicateNumbers_ReturnsFalse()
    {
        var wines = new List<Wine>
        {
            new() { EventId = "event-1", WineNumber = 1 },
            new() { EventId = "event-1", WineNumber = 1 }, // duplicate
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);

        var result = await CreateSut().ValidateWineNumbersAsync("event-1");

        result.ShouldBeFalse();
    }

    // ── AssignWineNumbersAsync ────────────────────────────────────

    [Fact]
    public async Task AssignWineNumbersAsync_NoPaidWines_ReturnsEmptyDictionary()
    {
        var wines = new List<Wine>
        {
            new() { WineId = "wine-1", EventId = "event-1", IsPaid = false }
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);

        var result = await CreateSut().AssignWineNumbersAsync("event-1");

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task AssignWineNumbersAsync_PaidWinesOrderedByCategory()
    {
        var wines = new List<Wine>
        {
            new() { WineId = "wine-red",   EventId = "event-1", IsPaid = true, Category = WineCategory.Rodvin },
            new() { WineId = "wine-white", EventId = "event-1", IsPaid = true, Category = WineCategory.Hvitvin },
            new() { WineId = "wine-rose",  EventId = "event-1", IsPaid = true, Category = WineCategory.Rosevin },
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);
        _wineRepo.Setup(r => r.UpdateWineAsync(It.IsAny<Wine>())).Returns(Task.CompletedTask);

        var result = await CreateSut().AssignWineNumbersAsync("event-1");

        result.Count.ShouldBe(3);
        // White wine gets number 1 (first in category order), rosé gets 2, red gets 3
        result["wine-white"].ShouldBe(1);
        result["wine-rose"].ShouldBe(2);
        result["wine-red"].ShouldBe(3);
    }

    [Fact]
    public async Task AssignWineNumbersAsync_NumbersStartAtOne()
    {
        var wines = new List<Wine>
        {
            new() { WineId = "wine-1", EventId = "event-1", IsPaid = true, Category = WineCategory.Hvitvin }
        };
        _wineRepo.Setup(r => r.GetAllWinesAsync()).ReturnsAsync(wines);
        _wineRepo.Setup(r => r.UpdateWineAsync(It.IsAny<Wine>())).Returns(Task.CompletedTask);

        var result = await CreateSut().AssignWineNumbersAsync("event-1");

        result["wine-1"].ShouldBe(1);
    }
}
