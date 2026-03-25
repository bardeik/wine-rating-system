using WineApp.Models;

namespace WineApp.Services;

public interface IWineJudgingService
{
    Task<IList<WineRating>> GetAllWineRatingsAsync();
    Task<WineRating?> GetWineRatingByIdAsync(string id);
    Task<IList<WineRating>> GetRatingsByJudgeAsync(string judgeId);
    Task<WineRating?> GetRatingByWineAndJudgeAsync(string wineId, string judgeId);
    Task AddWineRatingAsync(WineRating rating);
    Task UpdateWineRatingAsync(WineRating rating);
    Task DeleteWineRatingAsync(string id);
    Task<List<WineResult>> GetAllWineResultsAsync();
    Task<WineResult?> GetWineResultByIdAsync(string id);
    Task<WineResult?> GetWineResultByWineIdAsync(string wineId);
    Task AddWineResultAsync(WineResult result);
    Task UpdateWineResultAsync(WineResult result);
}
