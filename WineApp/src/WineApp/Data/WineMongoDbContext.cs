using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineMongoDbContext
{
    public const string WineProducersCollection = "wineproducers";
    public const string WinesCollection = "wines";
    public const string WineRatingsCollection = "wineratings";
    public const string EventsCollection = "events";
    public const string WineResultsCollection = "wineresults";
    public const string PaymentsCollection = "payments";
    public const string FlightsCollection = "flights";

    private readonly IMongoDatabase _database;

    public WineMongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
        var databaseName = configuration["MongoDbSettings:DatabaseName"] ?? "wineapp";
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<WineProducer> WineProducers =>
        _database.GetCollection<WineProducer>(WineProducersCollection);

    public IMongoCollection<Wine> Wines =>
        _database.GetCollection<Wine>(WinesCollection);

    public IMongoCollection<WineRating> WineRatings =>
        _database.GetCollection<WineRating>(WineRatingsCollection);

    public IMongoCollection<Event> Events =>
        _database.GetCollection<Event>(EventsCollection);

    public IMongoCollection<WineResult> WineResults =>
        _database.GetCollection<WineResult>(WineResultsCollection);

    public IMongoCollection<Payment> Payments =>
        _database.GetCollection<Payment>(PaymentsCollection);

    public IMongoCollection<Flight> Flights =>
        _database.GetCollection<Flight>(FlightsCollection);
}
