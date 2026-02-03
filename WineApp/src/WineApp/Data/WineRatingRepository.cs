using WineApp.Models;

namespace WineApp.Data;

public class WineRatingRepository : IWineRatingRepository
{
    private readonly List<WineRating> wineRatings = new()
    {
        new WineRating {
            WineRatingId = 1,
            JudgeId = "Hans",
            Nose = 4,
            Taste = 5,
            Visuality = 5,
            WineId = 1
        },
        new WineRating {
            WineRatingId = 2,
            JudgeId = "Petter",
            Nose = 3,
            Taste = 4,
            Visuality = 3,
            WineId = 1
        },
        new WineRating {
            WineRatingId = 3,
            JudgeId = "Frans",
            Nose = 5,
            Taste = 4,
            Visuality = 6,
            WineId = 1
        },
        new WineRating {
            WineRatingId = 4,
            JudgeId = "Ola",
            Nose = 5,
            Taste = 4,
            Visuality = 4,
            WineId = 1
        }
    };
    
    public IList<WineRating> GetAllWineRatings() => wineRatings;

    public WineRating? GetWineRatingById(int id) => 
        wineRatings.Find(wineRating => wineRating.WineRatingId == id);

    public int AddWineRating(WineRating wineRating)
    {
        var newId = wineRatings.Count > 0 ? wineRatings.Max(x => x.WineRatingId) + 1 : 1;
        wineRating.WineRatingId = newId;
        wineRatings.Add(wineRating);
        return newId;
    }

    public void DeleteWineRating(int id)
    {
        var rating = wineRatings.SingleOrDefault(x => x.WineRatingId == id);
        if (rating != null)
            wineRatings.Remove(rating);
    }
}
