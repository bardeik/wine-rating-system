using WineApp.Models;

namespace WineApp.Data;

public interface IWineRatingRepository
{
    Task<IList<WineRating>> GetAllWineRatingsAsync();
    Task<WineRating?> GetWineRatingByIdAsync(string id);
    Task<string> AddWineRatingAsync(WineRating wineRating);
    Task UpdateWineRatingAsync(WineRating wineRating);
    Task DeleteWineRatingAsync(string id);
}
