using MongoDB.Bson;
using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public sealed class AuditRetentionStore : IAuditRetentionStore
{
    private const string LockField = "AuditRetentionLock";
    private readonly IMongoDatabase database;

    public AuditRetentionStore(WineMongoDbContext context) : this(context.Database) { }
    public AuditRetentionStore(IMongoDatabase database) => this.database = database;

    public Task<List<Event>> GetEventsAsync(CancellationToken cancellationToken = default) =>
        database.GetCollection<Event>(WineMongoDbContext.EventsCollection).Find(_ => true)
            .Project<Event>(new BsonDocument("SecurityAudit", 0)).ToListAsync(cancellationToken);

    public Task<Event?> GetEventAsync(string eventId, CancellationToken cancellationToken = default) =>
        database.GetCollection<Event>(WineMongoDbContext.EventsCollection).Find(evt => evt.EventId == eventId)
            .Project<Event>(new BsonDocument("SecurityAudit", 0)).FirstOrDefaultAsync(cancellationToken)!;

    public async Task WithEventLockAsync(string eventId, Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var events = database.GetCollection<BsonDocument>(WineMongoDbContext.EventsCollection);
        var filters = Builders<BsonDocument>.Filter;
        var id = filters.Eq("_id", ObjectId.Parse(eventId));
        var token = Guid.NewGuid().ToString();
        var acquired = await events.UpdateOneAsync(id & filters.Exists(LockField, false),
            Builders<BsonDocument>.Update.Set(LockField, token), cancellationToken: cancellationToken);
        if (acquired.MatchedCount != 1)
            throw new InvalidOperationException("Audit maintenance is busy or the event is missing. Retry after maintenance completes.");
        var safeToRelease = false;
        try
        {
            await operation();
            safeToRelease = true;
        }
        catch (ArgumentException)
        {
            safeToRelease = true;
            throw;
        }
        finally
        {
            if (safeToRelease)
            {
                var released = await events.UpdateOneAsync(id & filters.Eq(LockField, token),
                    Builders<BsonDocument>.Update.Unset(LockField), cancellationToken: CancellationToken.None);
                if (released.MatchedCount != 1)
                    throw new InvalidOperationException("The audit maintenance lock could not be released. Operator review is required.");
            }
        }
    }

    public async Task<long> RemoveExpiredEntriesAsync(string eventId, DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        if (cutoffUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("A UTC cutoff is required.", nameof(cutoffUtc));
        var filters = Builders<BsonDocument>.Filter;
        var eventFilter = filters.Eq("EventId", eventId);
        var flights = await database.GetCollection<BsonDocument>(WineMongoDbContext.FlightsCollection)
            .Find(eventFilter).Project(new BsonDocument { { "_id", 1 }, { "WineIds", 1 } }).ToListAsync(cancellationToken);
        var wineDocuments = await database.GetCollection<BsonDocument>(WineMongoDbContext.WinesCollection)
            .Find(eventFilter).Project(new BsonDocument("_id", 1)).ToListAsync(cancellationToken);
        var wineIds = wineDocuments.Select(wine => wine["_id"].ToString()!)
            .Distinct().ToList();
        var flightIds = flights.Select(flight => flight["_id"].ToString()!).ToList();
        var legacyRating = filters.Exists("FlightId", false) | filters.Eq("FlightId", BsonNull.Value);
        var update = new BsonDocument("$pull", new BsonDocument("SecurityAudit",
            new BsonDocument("TimestampUtc", new BsonDocument("$lte", new BsonDateTime(cutoffUtc)))));
        long affected = 0;
        foreach (var (collection, filter) in new[]
        {
            (WineMongoDbContext.EventsCollection, filters.Eq("_id", ObjectId.Parse(eventId))),
            (WineMongoDbContext.FlightsCollection, eventFilter),
            (WineMongoDbContext.WineRatingsCollection, filters.In("FlightId", flightIds) | (legacyRating & filters.In("WineId", wineIds)))
        })
        {
            var result = await database.GetCollection<BsonDocument>(collection)
                .UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
            affected += result.ModifiedCount;
        }
        return affected;
    }
}