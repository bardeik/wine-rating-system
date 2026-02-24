using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineRepository : IWineRepository
{
    private readonly IMongoCollection<Wine> _collection;

    public WineRepository(WineMongoDbContext context) =>
        _collection = context.Wines;

    public IList<Wine> GetAllWines() =>
        _collection.Find(_ => true).ToList();

    public Wine? GetWineById(string id) =>
        _collection.Find(w => w.WineId == id).FirstOrDefault();

    public IList<Wine> GetAllWinesFromProducer(string producerId) =>
        _collection.Find(w => w.WineProducerId == producerId).ToList();

    public string AddWine(Wine wine)
    {
        _collection.InsertOne(wine);
        return wine.WineId;
    }

    public void DeleteWine(string id) =>
        _collection.DeleteOne(w => w.WineId == id);
}
