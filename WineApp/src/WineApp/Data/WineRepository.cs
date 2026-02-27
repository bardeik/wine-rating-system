using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineRepository : IWineRepository
{
    private readonly IMongoCollection<Wine> _collection;

    public WineRepository(WineMongoDbContext context) =>
        _collection = context.Wines;

    public async Task<IList<Wine>> GetAllWinesAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<Wine?> GetWineByIdAsync(string id) =>
        await _collection.Find(w => w.WineId == id).FirstOrDefaultAsync();

    public async Task<IList<Wine>> GetAllWinesFromProducerAsync(string producerId) =>
        await _collection.Find(w => w.WineProducerId == producerId).ToListAsync();

    public async Task<string> AddWineAsync(Wine wine)
    {
        await _collection.InsertOneAsync(wine);
        return wine.WineId;
    }

    public async Task UpdateWineAsync(Wine wine) =>
        await _collection.ReplaceOneAsync(w => w.WineId == wine.WineId, wine);

    public async Task DeleteWineAsync(string id) =>
        await _collection.DeleteOneAsync(w => w.WineId == id);
}
