using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace WineApp.Tests.Services;

public class AuditRetentionStoreTests
{
    private readonly Mock<IMongoDatabase> database = new();
    private readonly Mock<IMongoCollection<BsonDocument>> events = new();
    private readonly string eventId = ObjectId.GenerateNewId().ToString();

    public AuditRetentionStoreTests()
    {
        database.Setup(value => value.GetCollection<BsonDocument>(WineMongoDbContext.EventsCollection,
            It.IsAny<MongoCollectionSettings>())).Returns(events.Object);
    }

    private static readonly RenderArgs<BsonDocument> RenderArguments = new(
        BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>(), BsonSerializer.SerializerRegistry);

    [Fact]
    public async Task Lock_IsAcquiredAndReleasedWithoutExpiry()
    {
        var writes = new List<BsonDocument>();
        events.Setup(value => value.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<BsonDocument>, UpdateDefinition<BsonDocument>, UpdateOptions, CancellationToken>((filter, update, options, token) =>
                writes.Add(update.Render(RenderArguments).AsBsonDocument))
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));
        var invoked = false;
        await new AuditRetentionStore(database.Object).WithEventLockAsync(eventId, () => { invoked = true; return Task.CompletedTask; });
        invoked.ShouldBeTrue();
        writes.Count.ShouldBe(2);
        writes[0]["$set"].AsBsonDocument.Contains("AuditRetentionLock").ShouldBeTrue();
        writes[1]["$unset"].AsBsonDocument.Contains("AuditRetentionLock").ShouldBeTrue();
        writes[0].ToJson().ShouldNotContain("Expires");
    }

    [Fact]
    public async Task BusyLock_RejectsOperationWithoutReleasingAnotherLock()
    {
        events.Setup(value => value.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateResult.Acknowledged(0, 0, null));
        var invoked = false;
        await Should.ThrowAsync<InvalidOperationException>(() => new AuditRetentionStore(database.Object)
            .WithEventLockAsync(eventId, () => { invoked = true; return Task.CompletedTask; }));
        invoked.ShouldBeFalse();
        events.Invocations.Count.ShouldBe(1);
    }

    [Fact]
    public async Task UncertainFailure_PreservesLockForOperatorReview()
    {
        events.Setup(value => value.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));
        await Should.ThrowAsync<ApplicationException>(() => new AuditRetentionStore(database.Object)
            .WithEventLockAsync(eventId, () => throw new ApplicationException("Simulated failure")));
        events.Invocations.Count.ShouldBe(1);
    }

    [Fact]
    public async Task LocalValidationFailure_ReleasesOwnedLock()
    {
        events.Setup(value => value.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));
        await Should.ThrowAsync<ArgumentException>(() => new AuditRetentionStore(database.Object)
            .WithEventLockAsync(eventId, () => throw new ArgumentException("Invalid local input")));
        events.Invocations.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Cleanup_OnlyPullsExpiredAuditEntriesFromKnownEventDocuments()
    {
        var flightId = ObjectId.GenerateNewId();
        var wineId = ObjectId.GenerateNewId();
        var flights = CollectionWithDocuments([new BsonDocument { { "_id", flightId }, { "WineIds", new BsonArray { wineId.ToString() } } }]);
        var wines = CollectionWithDocuments([new BsonDocument("_id", wineId)]);
        var ratings = new Mock<IMongoCollection<BsonDocument>>();
        database.Setup(value => value.GetCollection<BsonDocument>(WineMongoDbContext.FlightsCollection, It.IsAny<MongoCollectionSettings>())).Returns(flights.Object);
        database.Setup(value => value.GetCollection<BsonDocument>(WineMongoDbContext.WinesCollection, It.IsAny<MongoCollectionSettings>())).Returns(wines.Object);
        database.Setup(value => value.GetCollection<BsonDocument>(WineMongoDbContext.WineRatingsCollection, It.IsAny<MongoCollectionSettings>())).Returns(ratings.Object);
        var writes = new List<(BsonDocument filter, BsonDocument update)>();
        foreach (var collection in new[] { events, flights, ratings })
        {
            collection.Setup(value => value.UpdateManyAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(),
                It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<BsonDocument>, UpdateDefinition<BsonDocument>, UpdateOptions, CancellationToken>((filter, update, options, token) =>
                    writes.Add((filter.Render(RenderArguments), update.Render(RenderArguments).AsBsonDocument)))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));
        }
        var cutoff = new DateTime(2024, 10, 23, 0, 0, 0, DateTimeKind.Utc);
        (await new AuditRetentionStore(database.Object).RemoveExpiredEntriesAsync(eventId, cutoff)).ShouldBe(3);
        writes.Count.ShouldBe(3);
        foreach (var write in writes)
        {
            write.update.Names.ShouldBe(new[] { "$pull" });
            write.update["$pull"]["SecurityAudit"]["TimestampUtc"]["$lte"].ToUniversalTime().ShouldBe(cutoff);
        }
        writes[2].filter.ToJson().ShouldContain(wineId.ToString());
        writes[2].filter.ToJson().ShouldContain(flightId.ToString());
        writes[2].filter.ToJson().ShouldContain("$exists");
        var legacyBranch = writes[2].filter["$or"].AsBsonArray[1].AsBsonDocument;
        legacyBranch.Contains("WineId").ShouldBeTrue();
        legacyBranch["$or"].AsBsonArray.Count.ShouldBe(2);
        database.Verify(value => value.GetCollection<BsonDocument>(WineMongoDbContext.PaymentsCollection, It.IsAny<MongoCollectionSettings>()), Times.Never);
        foreach (var collection in new[] { events, flights, wines, ratings })
            collection.Invocations.ShouldNotContain(invocation => invocation.Method.Name.StartsWith("Delete", StringComparison.Ordinal));
    }

    private static Mock<IMongoCollection<BsonDocument>> CollectionWithDocuments(List<BsonDocument> documents)
    {
        var cursor = new Mock<IAsyncCursor<BsonDocument>>();
        cursor.Setup(value => value.Current).Returns(documents);
        cursor.SetupSequence(value => value.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        var collection = new Mock<IMongoCollection<BsonDocument>>();
        collection.Setup(value => value.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), It.IsAny<CancellationToken>())).ReturnsAsync(cursor.Object);
        return collection;
    }
}