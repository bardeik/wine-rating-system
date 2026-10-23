using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public sealed class SecurityMutationStore : ISecurityMutationStore
{
    private readonly IMongoDatabase database;
    private readonly TimeProvider clock;

    public SecurityMutationStore(WineMongoDbContext context, TimeProvider clock) : this(context.Database, clock) { }

    public SecurityMutationStore(IMongoDatabase database, TimeProvider clock)
    {
        this.database = database;
        this.clock = clock;
    }

    private static readonly string[] Collections =
        [WineMongoDbContext.WineRatingsCollection, WineMongoDbContext.EventsCollection, WineMongoDbContext.FlightsCollection];

    public async Task CommitAsync<T>(string collection, string id, int? expectedRevision,
        T? before, T after, string actorId, string action) where T : class
    {
        if (!Collections.Contains(collection)) throw new ArgumentOutOfRangeException(nameof(collection));
        if (string.IsNullOrWhiteSpace(actorId)) throw new UnauthorizedAccessException();
        var documents = database.GetCollection<BsonDocument>(collection);
        var entry = new SecurityAuditEntry
        {
            ActorUserId = actorId, TimestampUtc = clock.GetUtcNow().UtcDateTime,
            TargetId = id, Action = action, Before = Snapshot(before), After = Snapshot(after)
        };
        var document = after.ToBsonDocument();
        document["SecurityRevision"] = (expectedRevision ?? 0) + 1;
        if (expectedRevision is null)
        {
            document["SecurityAudit"] = new BsonArray { entry.ToBsonDocument() };
            await documents.InsertOneAsync(document);
            return;
        }

        var filters = Builders<BsonDocument>.Filter;
        var revision = expectedRevision == 0
            ? filters.Or(filters.Eq("SecurityRevision", 0), filters.Exists("SecurityRevision", false))
            : filters.Eq("SecurityRevision", expectedRevision.Value);
        var filter = filters.Eq("_id", ObjectId.Parse(id)) & revision;
        document.Remove("_id");
        var update = new BsonDocument
        {
            { "$set", document },
            { "$push", new BsonDocument("SecurityAudit", entry.ToBsonDocument()) }
        };
        var result = await documents.UpdateOneAsync(filter, update);
        if (result.MatchedCount != 1)
            throw new InvalidOperationException("The record changed. Reload before trying again.");
    }

    public async Task<List<SecurityAuditEntry>> GetRecentAuditAsync()
    {
        var entries = new List<SecurityAuditEntry>();
        foreach (var collection in Collections)
        {
            var pipeline = new BsonDocument[]
            {
                new("$unwind", "$SecurityAudit"),
                new("$sort", new BsonDocument("SecurityAudit.TimestampUtc", -1)),
                new("$limit", 100),
                new("$replaceRoot", new BsonDocument("newRoot", "$SecurityAudit"))
            };
            var documents = await database.GetCollection<BsonDocument>(collection)
                .Aggregate<BsonDocument>(pipeline).ToListAsync();
            entries.AddRange(documents.Select(document => BsonSerializer.Deserialize<SecurityAuditEntry>(document)));
        }
        return entries.OrderByDescending(entry => entry.TimestampUtc).Take(100).ToList();
    }

    private static string Snapshot<T>(T? value) => value switch
    {
        WineRating rating => new BsonDocument
        {
            { "JudgeId", rating.JudgeId }, { "WineId", rating.WineId },
            { "Appearance", rating.Appearance }, { "Nose", rating.Nose }, { "Taste", rating.Taste },
            { "Comment", rating.Comment }, { "IsDeleted", rating.IsDeleted }
        }.ToJson(),
        Event evt => new BsonDocument
        {
            { "Name", evt.Name }, { "IsActive", evt.IsActive },
            { "EndedAtUtc", evt.EndedAtUtc is { } ended ? new BsonDateTime(ended) : BsonNull.Value },
            { "AuditDisputeReference", evt.AuditDisputeReference is { } reference ? new BsonString(reference) : BsonNull.Value },
            { "ResultsPublished", evt.ResultsPublished }, { "IsDeleted", evt.IsDeleted }
        }.ToJson(),
        Flight flight => new BsonDocument
        {
            { "EventId", flight.EventId }, { "WineIds", new BsonArray(flight.WineIds) },
            { "AssignedJudgeIds", new BsonArray(flight.AssignedJudgeIds) }, { "IsDeleted", flight.IsDeleted }
        }.ToJson(),
        null => string.Empty,
        _ => throw new ArgumentException("Unsupported audited entity.")
    };
}