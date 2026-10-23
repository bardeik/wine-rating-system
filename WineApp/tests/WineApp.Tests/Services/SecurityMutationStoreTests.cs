using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace WineApp.Tests.Services;

public class SecurityMutationStoreTests
{
    private readonly Mock<IMongoDatabase> database = new();
    private readonly Mock<IMongoCollection<BsonDocument>> collection = new();
    private readonly WineRating rating = new() { JudgeId = SecurityTestIdentity.UserId, WineId = ObjectId.GenerateNewId().ToString() };
    private readonly TimeProvider clock = FrozenTimeProvider.At(2026, 10, 23);

    public SecurityMutationStoreTests()
    {
        database.Setup(value => value.GetCollection<BsonDocument>(WineMongoDbContext.WineRatingsCollection,
            It.IsAny<MongoCollectionSettings>())).Returns(collection.Object);
    }

    private SecurityMutationStore CreateSut() => new(database.Object, clock);
    private static BsonDocument Render(UpdateDefinition<BsonDocument> update) => update.Render(
        new RenderArgs<BsonDocument>(BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>(), BsonSerializer.SerializerRegistry)).AsBsonDocument;

    [Fact]
    public async Task Create_InsertsScoreAndAuditTogether()
    {
        BsonDocument? inserted = null;
        collection.Setup(value => value.InsertOneAsync(It.IsAny<BsonDocument>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
            .Callback<BsonDocument, InsertOneOptions, CancellationToken>((document, options, token) => inserted = document)
            .Returns(Task.CompletedTask);
        await CreateSut().CommitAsync<WineRating>(WineMongoDbContext.WineRatingsCollection, rating.WineRatingId,
            null, null, rating, SecurityTestIdentity.UserId, "ScoreCreated");
        inserted.ShouldNotBeNull();
        inserted["SecurityRevision"].AsInt32.ShouldBe(1);
        var audit = inserted["SecurityAudit"].AsBsonArray.Single().AsBsonDocument;
        audit["ActorUserId"].AsString.ShouldBe(SecurityTestIdentity.UserId);
        audit["TimestampUtc"].ToUniversalTime().ShouldBe(clock.GetUtcNow().UtcDateTime);
        audit["Before"].AsString.ShouldBeEmpty();
        audit["After"].AsString.ShouldContain(rating.WineId);
        collection.Verify(value => value.InsertOneAsync(It.IsAny<BsonDocument>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_UsesSingleRevisionCheckedWriteAndAppendsAudit()
    {
        UpdateDefinition<BsonDocument>? update = null;
        FilterDefinition<BsonDocument>? filter = null;
        collection.Setup(value => value.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<UpdateDefinition<BsonDocument>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()))
            .Callback<FilterDefinition<BsonDocument>, UpdateDefinition<BsonDocument>, UpdateOptions, CancellationToken>((where, change, options, token) =>
            { filter = where; update = change; })
            .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));
        await CreateSut().CommitAsync(WineMongoDbContext.WineRatingsCollection, rating.WineRatingId,
            0, rating, rating, SecurityTestIdentity.UserId, "ScoreUpdated");
        update.ShouldNotBeNull();
        var rendered = Render(update);
        rendered["$set"]["SecurityRevision"].AsInt32.ShouldBe(1);
        rendered["$set"].AsBsonDocument.Contains("SecurityAudit").ShouldBeFalse();
        rendered["$push"]["SecurityAudit"]["Action"].AsString.ShouldBe("ScoreUpdated");
        filter.ShouldNotBeNull();
        filter.Render(new RenderArgs<BsonDocument>(BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>(),
            BsonSerializer.SerializerRegistry)).ToJson().ShouldContain("SecurityRevision");
        collection.Verify(value => value.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<UpdateDefinition<BsonDocument>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevisionConflict_IsNotAcknowledgedAsSuccess()
    {
        collection.Setup(value => value.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<UpdateDefinition<BsonDocument>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateResult.Acknowledged(0, 0, null));
        await Should.ThrowAsync<InvalidOperationException>(() => CreateSut().CommitAsync(WineMongoDbContext.WineRatingsCollection,
            rating.WineRatingId, 2, rating, rating, SecurityTestIdentity.UserId, "ScoreUpdated"));
    }

    [Fact]
    public async Task PersistenceFailure_IsNotHidden()
    {
        collection.Setup(value => value.InsertOneAsync(It.IsAny<BsonDocument>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated persistence failure"));
        await Should.ThrowAsync<InvalidOperationException>(() => CreateSut().CommitAsync<WineRating>(WineMongoDbContext.WineRatingsCollection,
            rating.WineRatingId, null, null, rating, SecurityTestIdentity.UserId, "ScoreCreated"));
    }

    [Fact]
    public async Task MissingActor_IsRejectedBeforeDatabaseWrite()
    {
        await Should.ThrowAsync<UnauthorizedAccessException>(() => CreateSut().CommitAsync<WineRating>(WineMongoDbContext.WineRatingsCollection,
            rating.WineRatingId, null, null, rating, "", "ScoreCreated"));
        collection.VerifyNoOtherCalls();
    }
}