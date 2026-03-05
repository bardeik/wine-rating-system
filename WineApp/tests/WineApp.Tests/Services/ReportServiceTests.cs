namespace WineApp.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IWineRepository>         _wineRepo     = new();
    private readonly Mock<IWineRatingRepository>   _ratingRepo   = new();
    private readonly Mock<IWineProducerRepository> _producerRepo = new();

    private ReportService CreateSut() =>
        new(_wineRepo.Object, _ratingRepo.Object, _producerRepo.Object);

    // -----------------------------------------------------------------------
    //  Tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetWineReportRowsAsync_WineWithThreeRatings_AveragesAreCorrect()
    {
        var wine = MakeWine("w1");
        _wineRepo.Setup(r => r.GetAllWinesAsync())
            .ReturnsAsync(new List<Wine> { wine });

        _ratingRepo.Setup(r => r.GetAllWineRatingsAsync())
            .ReturnsAsync(new List<WineRating>
            {
                MakeRating("w1", 1.0m, 2.0m, 7.0m),
                MakeRating("w1", 2.0m, 3.0m, 9.0m),
                MakeRating("w1", 3.0m, 4.0m, 11.0m)
            });

        _producerRepo.Setup(r => r.GetAllWineProducersAsync())
            .ReturnsAsync(new List<WineProducer>
            {
                new() { WineProducerId = "prod-1", WineyardName = "TestProducer" }
            });

        var rows = await CreateSut().GetWineReportRowsAsync();

        rows.Count.ShouldBe(1);
        var row = rows[0];
        row.AvgAppearance.ShouldBe(2.0, tolerance: 0.001);
        row.AvgNose.ShouldBe(3.0, tolerance: 0.001);
        row.AvgTaste.ShouldBe(9.0, tolerance: 0.001);
        row.Total.ShouldBe(14.0, tolerance: 0.001);
        row.RatingCount.ShouldBe(3);
    }

    [Fact]
    public async Task GetWineReportRowsAsync_WineWithNoRatings_AllAveragesAreZero()
    {
        var wine = MakeWine("w2");
        _wineRepo.Setup(r => r.GetAllWinesAsync())
            .ReturnsAsync(new List<Wine> { wine });

        _ratingRepo.Setup(r => r.GetAllWineRatingsAsync())
            .ReturnsAsync(new List<WineRating>());

        _producerRepo.Setup(r => r.GetAllWineProducersAsync())
            .ReturnsAsync(new List<WineProducer>
            {
                new() { WineProducerId = "prod-1", WineyardName = "TestProducer" }
            });

        var rows = await CreateSut().GetWineReportRowsAsync();

        var row = rows[0];
        row.AvgAppearance.ShouldBe(0.0);
        row.AvgNose.ShouldBe(0.0);
        row.AvgTaste.ShouldBe(0.0);
        row.Total.ShouldBe(0.0);
        row.RatingCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetWineReportRowsAsync_ProducerNameResolvedCorrectly()
    {
        var wine = MakeWine("w3", "prod-99");
        _wineRepo.Setup(r => r.GetAllWinesAsync())
            .ReturnsAsync(new List<Wine> { wine });

        _ratingRepo.Setup(r => r.GetAllWineRatingsAsync())
            .ReturnsAsync(new List<WineRating>());

        _producerRepo.Setup(r => r.GetAllWineProducersAsync())
            .ReturnsAsync(new List<WineProducer>
            {
                new() { WineProducerId = "prod-99", WineyardName = "Testgård" }
            });

        var rows = await CreateSut().GetWineReportRowsAsync();

        rows[0].ProducerName.ShouldBe("Testgård");
    }

    [Fact]
    public async Task GetWineReportRowsAsync_UnknownProducer_UsesPlaceholder()
    {
        var wine = MakeWine("w4", "nonexistent-producer");
        _wineRepo.Setup(r => r.GetAllWinesAsync())
            .ReturnsAsync(new List<Wine> { wine });

        _ratingRepo.Setup(r => r.GetAllWineRatingsAsync())
            .ReturnsAsync(new List<WineRating>());

        _producerRepo.Setup(r => r.GetAllWineProducersAsync())
            .ReturnsAsync(new List<WineProducer>
            {
                new() { WineProducerId = "prod-1", WineyardName = "SomeOtherProducer" }
            });

        var rows = await CreateSut().GetWineReportRowsAsync();

        rows[0].ProducerName.ShouldBe("–");
    }

    [Fact]
    public async Task GetWineReportRowsAsync_MultipleWines_OrderedByTotalDescending()
    {
        var wineA = MakeWine("wA");
        var wineB = MakeWine("wB");

        _wineRepo.Setup(r => r.GetAllWinesAsync())
            .ReturnsAsync(new List<Wine> { wineA, wineB });

        // Wine A: avg = 2.0 + 3.0 + 9.0 = 14.0
        // Wine B: avg = 1.0 + 2.0 + 5.0 = 8.0
        _ratingRepo.Setup(r => r.GetAllWineRatingsAsync())
            .ReturnsAsync(new List<WineRating>
            {
                MakeRating("wA", 2.0m, 3.0m, 9.0m),
                MakeRating("wB", 1.0m, 2.0m, 5.0m)
            });

        _producerRepo.Setup(r => r.GetAllWineProducersAsync())
            .ReturnsAsync(new List<WineProducer>
            {
                new() { WineProducerId = "prod-1", WineyardName = "TestProducer" }
            });

        var rows = await CreateSut().GetWineReportRowsAsync();

        rows.Count.ShouldBe(2);
        rows[0].RatingName.ShouldBe("RN-wA");
        rows[0].Total.ShouldBe(14.0, tolerance: 0.001);
        rows[1].RatingName.ShouldBe("RN-wB");
        rows[1].Total.ShouldBe(8.0, tolerance: 0.001);
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private static Wine MakeWine(string wineId, string producerId = "prod-1") => new Wine
    {
        WineId         = wineId,
        WineProducerId = producerId,
        Name           = $"Wine {wineId}",
        RatingName     = $"RN-{wineId}"
    };

    private static WineRating MakeRating(string wineId, decimal appearance, decimal nose, decimal taste) => new WineRating
    {
        WineId     = wineId,
        Appearance = appearance,
        Nose       = nose,
        Taste      = taste,
        JudgeId    = "judge-1"
    };
}
