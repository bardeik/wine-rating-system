using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineProducerRepository : IWineProducerRepository
{
    private readonly IMongoCollection<WineProducer> _collection;

    public WineProducerRepository(WineMongoDbContext context) =>
        _collection = context.WineProducers;

    public async Task<IList<WineProducer>> GetAllWineProducersAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<WineProducer?> GetWineProducerByIdAsync(string id) =>
        await _collection.Find(p => p.WineProducerId == id).FirstOrDefaultAsync();

    public async Task<string> AddWineProducerAsync(WineProducer wineProducer)
    {
        await _collection.InsertOneAsync(wineProducer);
        return wineProducer.WineProducerId;
    }

    public async Task UpdateWineProducerAsync(WineProducer wineProducer) =>
        await _collection.ReplaceOneAsync(p => p.WineProducerId == wineProducer.WineProducerId, wineProducer);

    public async Task DeleteWineProducerAsync(string id) =>
        await _collection.DeleteOneAsync(p => p.WineProducerId == id);

    public async Task<WineProducer?> GetWineProducerByUserIdAsync(string userId) =>
        await _collection.Find(p => p.UserId == userId).FirstOrDefaultAsync();
}
