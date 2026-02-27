using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineRatingRepository : IWineRatingRepository
{
    private readonly IMongoCollection<WineRating> _collection;

    public WineRatingRepository(WineMongoDbContext context) =>
        _collection = context.WineRatings;

    public async Task<IList<WineRating>> GetAllWineRatingsAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<WineRating?> GetWineRatingByIdAsync(string id) =>
        await _collection.Find(r => r.WineRatingId == id).FirstOrDefaultAsync();

    public async Task<string> AddWineRatingAsync(WineRating wineRating)
    {
        await _collection.InsertOneAsync(wineRating);
        return wineRating.WineRatingId;
    }

    public async Task UpdateWineRatingAsync(WineRating wineRating) =>
        await _collection.ReplaceOneAsync(r => r.WineRatingId == wineRating.WineRatingId, wineRating);

    public async Task DeleteWineRatingAsync(string id) =>
        await _collection.DeleteOneAsync(r => r.WineRatingId == id);
}
