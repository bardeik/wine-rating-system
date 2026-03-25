namespace WineApp.Tests.Services;

public class WineJudgingServiceTests
{
    private readonly Mock<IWineRatingRepository> _ratingRepo = new();
    private readonly Mock<IWineResultRepository> _resultRepo = new();

    private WineJudgingService CreateSut() => new(_ratingRepo.Object, _resultRepo.Object);

    // ── GetRatingsByJudgeAsync ────────────────────────────────────

    [Fact]
    public async Task GetRatingsByJudgeAsync_DelegatesToRepository()
    {
        const string judgeId = "judge-1";
        var ratings = new List<WineRating>
        {
            new() { JudgeId = judgeId, WineId = "wine-1" },
            new() { JudgeId = judgeId, WineId = "wine-2" },
        };

        _ratingRepo.Setup(r => r.GetRatingsByJudgeAsync(judgeId)).ReturnsAsync(ratings);

        var result = await CreateSut().GetRatingsByJudgeAsync(judgeId);

        result.ShouldBe(ratings);
        _ratingRepo.Verify(r => r.GetRatingsByJudgeAsync(judgeId), Times.Once);
    }

    [Fact]
    public async Task GetRatingsByJudgeAsync_NoRatings_ReturnsEmptyList()
    {
        const string judgeId = "judge-no-ratings";
        _ratingRepo.Setup(r => r.GetRatingsByJudgeAsync(judgeId)).ReturnsAsync([]);

        var result = await CreateSut().GetRatingsByJudgeAsync(judgeId);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetRatingsByJudgeAsync_PassesJudgeIdCorrectly()
    {
        const string judgeId = "specific-judge-id";
        _ratingRepo.Setup(r => r.GetRatingsByJudgeAsync(judgeId)).ReturnsAsync([]);

        await CreateSut().GetRatingsByJudgeAsync(judgeId);

        _ratingRepo.Verify(r => r.GetRatingsByJudgeAsync(judgeId), Times.Once);
        _ratingRepo.Verify(r => r.GetRatingsByJudgeAsync(It.Is<string>(s => s != judgeId)), Times.Never);
    }

    // ── GetRatingByWineAndJudgeAsync ──────────────────────────────

    [Fact]
    public async Task GetRatingByWineAndJudgeAsync_ExistingRating_ReturnsRating()
    {
        const string wineId  = "wine-1";
        const string judgeId = "judge-1";
        var rating = new WineRating { WineId = wineId, JudgeId = judgeId, Appearance = 2.0m, Nose = 3.0m, Taste = 8.0m };

        _ratingRepo.Setup(r => r.GetRatingByWineAndJudgeAsync(wineId, judgeId)).ReturnsAsync(rating);

        var result = await CreateSut().GetRatingByWineAndJudgeAsync(wineId, judgeId);

        result.ShouldNotBeNull();
        result.WineId.ShouldBe(wineId);
        result.JudgeId.ShouldBe(judgeId);
    }

    [Fact]
    public async Task GetRatingByWineAndJudgeAsync_NotFound_ReturnsNull()
    {
        _ratingRepo.Setup(r => r.GetRatingByWineAndJudgeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((WineRating?)null);

        var result = await CreateSut().GetRatingByWineAndJudgeAsync("wine-x", "judge-x");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetRatingByWineAndJudgeAsync_DelegatesToRepository()
    {
        const string wineId  = "wine-42";
        const string judgeId = "judge-99";
        _ratingRepo.Setup(r => r.GetRatingByWineAndJudgeAsync(wineId, judgeId))
            .ReturnsAsync((WineRating?)null);

        await CreateSut().GetRatingByWineAndJudgeAsync(wineId, judgeId);

        _ratingRepo.Verify(r => r.GetRatingByWineAndJudgeAsync(wineId, judgeId), Times.Once);
    }
}
