namespace WineApp.Tests.Services;

public class WineCatalogServiceTests
{
    private readonly Mock<IWineRepository>         _wineRepo     = new();
    private readonly Mock<IWineProducerRepository> _producerRepo = new();

    private WineCatalogService CreateSut() => new(_wineRepo.Object, _producerRepo.Object);

    // ── GetWinesByIdsAsync ────────────────────────────────────────

    [Fact]
    public async Task GetWinesByIdsAsync_DelegatesToRepository()
    {
        var ids = new[] { "wine-1", "wine-2" };
        var wines = new List<Wine>
        {
            new() { WineId = "wine-1", Name = "Riesling" },
            new() { WineId = "wine-2", Name = "Chardonnay" },
        };

        _wineRepo.Setup(r => r.GetWinesByIdsAsync(ids)).ReturnsAsync(wines);

        var result = await CreateSut().GetWinesByIdsAsync(ids);

        result.ShouldBe(wines);
        _wineRepo.Verify(r => r.GetWinesByIdsAsync(ids), Times.Once);
    }

    [Fact]
    public async Task GetWinesByIdsAsync_EmptyInput_ReturnsEmptyList()
    {
        var ids = Array.Empty<string>();
        _wineRepo.Setup(r => r.GetWinesByIdsAsync(ids)).ReturnsAsync([]);

        var result = await CreateSut().GetWinesByIdsAsync(ids);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetWinesByIdsAsync_UnknownIds_ReturnsEmptyList()
    {
        var ids = new[] { "does-not-exist" };
        _wineRepo.Setup(r => r.GetWinesByIdsAsync(ids)).ReturnsAsync([]);

        var result = await CreateSut().GetWinesByIdsAsync(ids);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetWinesByIdsAsync_PartialMatch_ReturnsOnlyMatchedWines()
    {
        var ids = new[] { "wine-1", "wine-missing" };
        var wines = new List<Wine> { new() { WineId = "wine-1" } };

        _wineRepo.Setup(r => r.GetWinesByIdsAsync(ids)).ReturnsAsync(wines);

        var result = await CreateSut().GetWinesByIdsAsync(ids);

        result.Count.ShouldBe(1);
        result[0].WineId.ShouldBe("wine-1");
    }
}
