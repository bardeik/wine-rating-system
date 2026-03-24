using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class WineJudgingService : IWineJudgingService
{
    private readonly IWineRatingRepository _wineRatingRepository;
    private readonly IWineResultRepository _wineResultRepository;

    public WineJudgingService(IWineRatingRepository wineRatingRepository, IWineResultRepository wineResultRepository)
    {
        _wineRatingRepository = wineRatingRepository;
        _wineResultRepository = wineResultRepository;
    }

    public Task<IList<WineRating>> GetAllWineRatingsAsync() => _wineRatingRepository.GetAllWineRatingsAsync();
    public Task<WineRating?> GetWineRatingByIdAsync(string id) => _wineRatingRepository.GetWineRatingByIdAsync(id);
    public Task<IList<WineRating>> GetRatingsByJudgeAsync(string judgeId) => _wineRatingRepository.GetRatingsByJudgeAsync(judgeId);
    public Task<WineRating?> GetRatingByWineAndJudgeAsync(string wineId, string judgeId) => _wineRatingRepository.GetRatingByWineAndJudgeAsync(wineId, judgeId);
    public Task AddWineRatingAsync(WineRating rating) => _wineRatingRepository.AddWineRatingAsync(rating);
    public Task UpdateWineRatingAsync(WineRating rating) => _wineRatingRepository.UpdateWineRatingAsync(rating);
    public Task DeleteWineRatingAsync(string id) => _wineRatingRepository.DeleteWineRatingAsync(id);
    public Task<List<WineResult>> GetAllWineResultsAsync() => _wineResultRepository.GetAllWineResultsAsync();
    public Task<WineResult?> GetWineResultByIdAsync(string id) => _wineResultRepository.GetWineResultByIdAsync(id);
    public Task<WineResult?> GetWineResultByWineIdAsync(string wineId) => _wineResultRepository.GetWineResultByWineIdAsync(wineId);
    public Task AddWineResultAsync(WineResult result) => _wineResultRepository.AddWineResultAsync(result);
    public Task UpdateWineResultAsync(WineResult result) => _wineResultRepository.UpdateWineResultAsync(result);
}
