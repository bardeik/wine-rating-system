using WineApp.Models;

namespace WineApp.Data;

public interface IWineRatingRepository
{
    IList<WineRating> GetAllWineRatings();
    WineRating? GetWineRatingById(string id);
    string AddWineRating(WineRating wineRating);
    void UpdateWineRating(WineRating wineRating);
    void DeleteWineRating(string id);
}
