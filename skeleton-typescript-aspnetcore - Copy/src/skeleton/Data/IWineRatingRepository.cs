using System.Collections.Generic;
using skeleton.Models;

namespace skeleton.Data
{
    public interface IWineRatingRepository
    {
        IList<WineRating> GetAllWineRatings();
        IList<WineRatingScore> GetScoreForWines();
        WineRating GetWineRatingById(int id);
        int AddWineRating(WineRating wineRating);
        void DeleteWineRating(int id);
    }
}
