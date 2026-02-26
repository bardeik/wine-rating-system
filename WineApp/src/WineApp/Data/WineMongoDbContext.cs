using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineMongoDbContext
{
    private readonly IMongoDatabase _database;

    public WineMongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var databaseName = configuration["MongoDbSettings:DatabaseName"] ?? "wineapp";
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<WineProducer> WineProducers =>
        _database.GetCollection<WineProducer>("wineproducers");

    public IMongoCollection<Wine> Wines =>
        _database.GetCollection<Wine>("wines");

    public IMongoCollection<WineRating> WineRatings =>
        _database.GetCollection<WineRating>("wineratings");

    public IMongoCollection<Event> Events =>
        _database.GetCollection<Event>("events");

    public IMongoCollection<WineResult> WineResults =>
        _database.GetCollection<WineResult>("wineresults");

    public IMongoCollection<Payment> Payments =>
        _database.GetCollection<Payment>("payments");

    public IMongoCollection<Flight> Flights =>
        _database.GetCollection<Flight>("flights");
}
