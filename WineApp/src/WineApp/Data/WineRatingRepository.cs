using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineRatingRepository : IWineRatingRepository
{
    private readonly IMongoCollection<WineRating> _collection;

    public WineRatingRepository(WineMongoDbContext context) =>
        _collection = context.WineRatings;

    public IList<WineRating> GetAllWineRatings() =>
        _collection.Find(_ => true).ToList();

    public WineRating? GetWineRatingById(string id) =>
        _collection.Find(r => r.WineRatingId == id).FirstOrDefault();

    public string AddWineRating(WineRating wineRating)
    {
        _collection.InsertOne(wineRating);
        return wineRating.WineRatingId;
    }

    public void UpdateWineRating(WineRating wineRating) =>
        _collection.ReplaceOne(r => r.WineRatingId == wineRating.WineRatingId, wineRating);

    public void DeleteWineRating(string id) =>
        _collection.DeleteOne(r => r.WineRatingId == id);
}
