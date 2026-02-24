using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineProducerRepository : IWineProducerRepository
{
    private readonly IMongoCollection<WineProducer> _collection;

    public WineProducerRepository(WineMongoDbContext context) =>
        _collection = context.WineProducers;

    public IList<WineProducer> GetAllWineProducers() =>
        _collection.Find(_ => true).ToList();

    public WineProducer? GetWineProducerById(string id) =>
        _collection.Find(p => p.WineProducerId == id).FirstOrDefault();

    public string AddWineProducer(WineProducer wineProducer)
    {
        _collection.InsertOne(wineProducer);
        return wineProducer.WineProducerId;
    }

    public void DeleteWineProducer(string id) =>
        _collection.DeleteOne(p => p.WineProducerId == id);
}
