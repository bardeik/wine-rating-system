using WineApp.Models;

namespace WineApp.Data;

public interface IWineRatingRepository
{
    IList<WineRating> GetAllWineRatings();
    WineRating? GetWineRatingById(int id);
    int AddWineRating(WineRating wineRating);
    void DeleteWineRating(int id);
}
