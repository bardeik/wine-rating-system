using WineApp.Shared.Dtos;

namespace WineApp.Shared.MobileServices;

public interface IMobileRatingService
{
    Task<IList<WineRatingDto>> GetMyRatingsAsync();
    Task<WineRatingDto?> GetRatingForWineAsync(string wineId, string judgeId);
    Task<bool> SaveRatingAsync(WineRatingDto rating);
}
