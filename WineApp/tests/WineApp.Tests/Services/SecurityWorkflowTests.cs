using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using WineApp.Extensions;

namespace WineApp.Tests.Services;

public class SecurityWorkflowTests
{
    private readonly Mock<IEventRepository> events = new();
    private readonly Mock<IFlightRepository> flights = new();
    private readonly Mock<IWineRepository> wines = new();
    private readonly Mock<IWineRatingRepository> ratings = new();
    private readonly Mock<IWineResultRepository> results = new();
    private readonly Mock<ISecurityMutationStore> mutations = new();
    private readonly Mock<UserManager<ApplicationUser>> users = new(Mock.Of<IUserStore<ApplicationUser>>(),
        null!, null!, null!, null!, null!, null!, null!, null!);
    private readonly Event activeEvent = new() { EventId = "event", IsActive = true };
    private readonly Flight assigned = new()
    {
        FlightId = "flight", EventId = "event", WineIds = ["wine"], AssignedJudgeIds = [SecurityTestIdentity.UserId]
    };
    private readonly Wine wine = new()
    {
        WineId = "wine", EventId = "event", IsPaid = true, WineNumber = 1,
        Name = "Private wine name", WineProducerId = "private-producer", RatingName = "Anonymous 1"
    };

    public SecurityWorkflowTests()
    {
        events.Setup(repo => repo.GetActiveEventAsync()).ReturnsAsync(activeEvent);
        events.Setup(repo => repo.GetEventByIdAsync("event")).ReturnsAsync(activeEvent);
        events.Setup(repo => repo.GetAllEventsAsync()).ReturnsAsync([activeEvent]);
        flights.Setup(repo => repo.GetFlightByIdAsync("flight")).ReturnsAsync(assigned);
        flights.Setup(repo => repo.GetFlightsForEventAsync("event")).ReturnsAsync([assigned]);
        wines.Setup(repo => repo.GetWineByIdAsync("wine")).ReturnsAsync(wine);
        wines.Setup(repo => repo.GetAllWinesAsync()).ReturnsAsync(new List<Wine> { wine });
        ratings.Setup(repo => repo.GetAllWineRatingsAsync()).ReturnsAsync(new List<WineRating>());
        results.Setup(repo => repo.GetAllWineResultsAsync()).ReturnsAsync([new WineResult { WineId = "wine" }]);
    }

    private SecurityAccess Access(string role) => SecurityTestIdentity.Create(events.Object, role.Length == 0 ? [] : [role]);
    private WineJudgingService Judging(string role = "Judge") => new(ratings.Object, results.Object,
        wines.Object, flights.Object, events.Object, Access(role), mutations.Object, FrozenTimeProvider.At(2026, 10, 23));
    private FlightService FlightService(string role = "Judge") =>
        new(wines.Object, flights.Object, Access(role), events.Object, mutations.Object);
    private SecurityAdministrationService Administration(string role) => new(Access(role), mutations.Object,
        events.Object, flights.Object, ratings.Object, wines.Object, users.Object, FrozenTimeProvider.At(2026, 10, 23));
    private static WineRating NewRating() => new()
    {
        WineId = "wine", FlightId = "flight", JudgeId = "untrusted-client-value", Appearance = 2, Nose = 3, Taste = 10
    };

    [Theory]
    [InlineData("")]
    [InlineData("WineProducer")]
    [InlineData("Judge")]
    [InlineData("Viewer")]
    public async Task UnpublishedResults_AreDeniedToEveryNonAdmin(string role)
    {
        await Should.ThrowAsync<UnauthorizedAccessException>(() => Access(role).RequireResultsAsync("event"));
        (await Judging(role).GetAllWineResultsAsync()).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("WineProducer")]
    [InlineData("Judge")]
    [InlineData("Viewer")]
    [InlineData("Admin")]
    public async Task PublishedResults_AreVisible(string role)
    {
        activeEvent.ResultsPublished = true;
        await Access(role).RequireResultsAsync("event");
        (await Judging(role).GetAllWineResultsAsync()).Count.ShouldBe(1);
    }

    [Fact]
    public async Task Admin_CanPreviewUnpublishedResults()
    {
        await Access("Admin").RequireResultsAsync("event");
        (await Judging("Admin").GetAllWineResultsAsync()).Count.ShouldBe(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("WineProducer")]
    [InlineData("Judge")]
    [InlineData("Viewer")]
    public async Task NonAdmin_CannotReadAuditOrChangePublication(string role)
    {
        await Should.ThrowAsync<UnauthorizedAccessException>(() => Administration(role).GetRecentAuditAsync());
        await Should.ThrowAsync<UnauthorizedAccessException>(() => Administration(role).SetPublicationAsync("event", true));
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Admin_CanReadAudit()
    {
        mutations.Setup(store => store.GetRecentAuditAsync()).ReturnsAsync([new SecurityAuditEntry { Action = "ScoreCreated" }]);
        (await Administration("Admin").GetRecentAuditAsync()).Single().Action.ShouldBe("ScoreCreated");
    }

    [Fact]
    public async Task Publication_IsAuditedAndUsesInjectedClock()
    {
        Event? saved = null;
        mutations.Setup(store => store.CommitAsync(WineMongoDbContext.EventsCollection, "event", 0,
            activeEvent, It.IsAny<Event>(), SecurityTestIdentity.UserId, "ResultsPublished"))
            .Callback<string, string, int?, Event?, Event, string, string>((collection, id, revision, before, after, actor, action) => saved = after)
            .Returns(Task.CompletedTask);
        await Administration("Admin").SetPublicationAsync("event", true);
        saved.ShouldNotBeNull();
        saved.ResultsPublished.ShouldBeTrue();
        saved.PublishedByUserId.ShouldBe(SecurityTestIdentity.UserId);
        saved.PublishedAtUtc.ShouldBe(new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc));
        activeEvent.ResultsPublished.ShouldBeFalse();
    }

    [Fact]
    public async Task Unpublishing_BlocksSubsequentReads()
    {
        activeEvent.ResultsPublished = true;
        (await Judging("Viewer").GetAllWineResultsAsync()).Count.ShouldBe(1);
        activeEvent.ResultsPublished = false;
        (await Judging("Viewer").GetAllWineResultsAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task AssignedFlight_ReturnsAnonymizedWine()
    {
        (await FlightService().GetFlightsForEventAsync("event")).Count.ShouldBe(1);
        var visible = (await FlightService().GetWinesInFlightAsync("flight")).Single();
        visible.RatingName.ShouldBe("Anonymous 1");
        visible.Name.ShouldBeEmpty();
        visible.WineProducerId.ShouldBeEmpty();
    }

    [Fact]
    public async Task UnassignedFlight_IsNotVisibleOrReadable()
    {
        assigned.AssignedJudgeIds.Clear();
        (await FlightService().GetFlightsForEventAsync("event")).ShouldBeEmpty();
        await Should.ThrowAsync<UnauthorizedAccessException>(() => FlightService().GetWinesInFlightAsync("flight"));
    }

    [Fact]
    public async Task EligibleScore_IsAuditedWithServerSideIdentity()
    {
        var rating = NewRating();
        await Judging().AddWineRatingAsync(rating);
        rating.JudgeId.ShouldBe(SecurityTestIdentity.UserId);
        rating.SecurityRevision.ShouldBe(1);
        mutations.Verify(store => store.CommitAsync(WineMongoDbContext.WineRatingsCollection,
            rating.WineRatingId, null, (WineRating?)null,
            It.Is<WineRating>(item => item.JudgeId == SecurityTestIdentity.UserId && item.Taste == 10),
            SecurityTestIdentity.UserId, "ScoreCreated"), Times.Once);
    }

    [Theory]
    [InlineData("unassigned")]
    [InlineData("unpaid")]
    [InlineData("unnumbered")]
    [InlineData("inactive")]
    [InlineData("wrong-event")]
    [InlineData("wrong-wine")]
    [InlineData("missing-flight")]
    public async Task IneligibleScore_IsRejectedWithoutWriting(string condition)
    {
        var rating = NewRating();
        switch (condition)
        {
            case "unassigned": assigned.AssignedJudgeIds.Clear(); break;
            case "unpaid": wine.IsPaid = false; break;
            case "unnumbered": wine.WineNumber = null; break;
            case "inactive": events.Setup(repo => repo.GetActiveEventAsync()).ReturnsAsync((Event?)null); break;
            case "wrong-event": wine.EventId = "other"; break;
            case "wrong-wine": assigned.WineIds.Clear(); break;
            case "missing-flight": rating.FlightId = null; break;
        }
        await Should.ThrowAsync<UnauthorizedAccessException>(() => Judging().AddWineRatingAsync(rating));
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ScoreOutsideDomainRange_IsRejected()
    {
        var rating = NewRating();
        rating.Taste = 14;
        await Should.ThrowAsync<ValidationException>(() => Judging().AddWineRatingAsync(rating));
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RevokedAssignment_PreventsLaterAutosave()
    {
        var rating = NewRating();
        var service = Judging();
        await service.AddWineRatingAsync(rating);
        ratings.Setup(repo => repo.GetWineRatingByIdAsync(rating.WineRatingId)).ReturnsAsync(rating);
        assigned.AssignedJudgeIds.Clear();
        await Should.ThrowAsync<UnauthorizedAccessException>(() => service.UpdateWineRatingAsync(rating));
    }

    [Fact]
    public async Task AnotherJudgesScore_CannotBeUpdated()
    {
        var stored = NewRating();
        stored.JudgeId = "22222222-2222-2222-2222-222222222222";
        ratings.Setup(repo => repo.GetWineRatingByIdAsync(stored.WineRatingId)).ReturnsAsync(stored);
        await Should.ThrowAsync<UnauthorizedAccessException>(() => Judging().UpdateWineRatingAsync(stored));
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task StaleScoreRevision_CannotOverwriteNewerScore()
    {
        var stored = NewRating();
        stored.SecurityRevision = 2;
        var stale = NewRating();
        stale.WineRatingId = stored.WineRatingId;
        ratings.Setup(repo => repo.GetWineRatingByIdAsync(stored.WineRatingId)).ReturnsAsync(stored);
        await Should.ThrowAsync<InvalidOperationException>(() => Judging().UpdateWineRatingAsync(stale));
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AdminDeletion_IsSoftAndAudited()
    {
        var stored = NewRating();
        ratings.Setup(repo => repo.GetWineRatingByIdAsync(stored.WineRatingId)).ReturnsAsync(stored);
        await Judging("Admin").DeleteWineRatingAsync(stored.WineRatingId);
        mutations.Verify(store => store.CommitAsync(WineMongoDbContext.WineRatingsCollection, stored.WineRatingId,
            0, stored, It.Is<WineRating>(item => item.IsDeleted), SecurityTestIdentity.UserId, "ScoreDeleted"), Times.Once);
        stored.IsDeleted.ShouldBeFalse();
        ratings.Verify(repo => repo.DeleteWineRatingAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DownloadFilter_BlocksUnpublishedResultsBeforeExport()
    {
        var filter = new DownloadSecurityFilter(Access("Viewer"), NullLogger<DownloadSecurityFilter>.Instance);
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), "event");
        var invoked = false;
        var response = await filter.InvokeAsync(context, _ => { invoked = true; return ValueTask.FromResult<object?>(null); });
        invoked.ShouldBeFalse();
        ((IStatusCodeHttpResult)response!).StatusCode.ShouldBe(403);
    }

    [Fact]
    public void SensitiveRoutes_AreAdminOnly()
    {
        foreach (var page in new[] { typeof(WineApp.Pages.Reports), typeof(WineApp.Pages.WineRatings), typeof(WineApp.Pages.AuditHistory) })
            page.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single().Roles.ShouldBe("Admin");
    }

    [Fact]
    public async Task AdminAssignment_UsesCurrentJudgeIdsAndIsAudited()
    {
        users.Setup(manager => manager.GetUsersInRoleAsync("Judge")).ReturnsAsync(new List<ApplicationUser>
        {
            new() { Id = Guid.Parse(SecurityTestIdentity.UserId) }
        });
        await Administration("Admin").AssignJudgesAsync("flight", [SecurityTestIdentity.UserId]);
        mutations.Verify(store => store.CommitAsync(WineMongoDbContext.FlightsCollection, "flight", 0, assigned,
            It.Is<Flight>(flight => flight.AssignedJudgeIds.SequenceEqual(new[] { SecurityTestIdentity.UserId })),
            SecurityTestIdentity.UserId, "FlightAssignmentsChanged"), Times.Once);
    }

    [Fact]
    public async Task NonJudgeAssignment_IsRejected()
    {
        users.Setup(manager => manager.GetUsersInRoleAsync("Judge")).ReturnsAsync(new List<ApplicationUser>());
        await Should.ThrowAsync<ArgumentException>(() => Administration("Admin").AssignJudgesAsync("flight", ["unknown-user"]));
        mutations.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Judge")]
    [InlineData("WineProducer")]
    [InlineData("Viewer")]
    public async Task NonAdmin_CannotAssignOrMigrate(string role)
    {
        await Should.ThrowAsync<UnauthorizedAccessException>(() => Administration(role).AssignJudgesAsync("flight", []));
        await Should.ThrowAsync<UnauthorizedAccessException>(() => Administration(role).MigrateLegacyRatingsAsync());
        mutations.VerifyNoOtherCalls();
        users.Verify(manager => manager.GetUsersInRoleAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UniqueLegacyIdentity_IsMigratedWithAudit()
    {
        var legacy = NewRating();
        legacy.JudgeId = "Legacy judge";
        legacy.FlightId = null;
        ratings.Setup(repo => repo.GetAllWineRatingsAsync()).ReturnsAsync(new List<WineRating> { legacy });
        users.Setup(manager => manager.GetUsersInRoleAsync("Judge")).ReturnsAsync(new List<ApplicationUser>
        {
            new() { Id = Guid.Parse(SecurityTestIdentity.UserId), DisplayName = "Legacy judge" }
        });
        (await Administration("Admin").MigrateLegacyRatingsAsync()).ShouldBeEmpty();
        mutations.Verify(store => store.CommitAsync(WineMongoDbContext.WineRatingsCollection, legacy.WineRatingId,
            0, legacy, It.Is<WineRating>(rating => rating.JudgeId == SecurityTestIdentity.UserId && rating.FlightId == "flight"),
            SecurityTestIdentity.UserId, "LegacyJudgeIdentityMigrated"), Times.Once);
        legacy.JudgeId.ShouldBe("Legacy judge");
    }

    [Fact]
    public async Task AmbiguousLegacyIdentity_RequiresReviewWithoutWriting()
    {
        var legacy = NewRating();
        legacy.JudgeId = "Shared name";
        legacy.FlightId = null;
        ratings.Setup(repo => repo.GetAllWineRatingsAsync()).ReturnsAsync(new List<WineRating> { legacy });
        users.Setup(manager => manager.GetUsersInRoleAsync("Judge")).ReturnsAsync(new List<ApplicationUser>
        {
            new() { Id = Guid.Parse(SecurityTestIdentity.UserId), DisplayName = "Shared name" },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), DisplayName = "Shared name" }
        });
        (await Administration("Admin").MigrateLegacyRatingsAsync()).ShouldBe(new[] { legacy.WineRatingId });
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ViewerCannotRecalculateResults()
    {
        var aggregation = new ScoreAggregationService(ratings.Object, wines.Object, events.Object, results.Object,
            new ClassificationService(), TimeProvider.System, Access("Viewer"));
        await Should.ThrowAsync<UnauthorizedAccessException>(() => aggregation.RecalculateEventResultsAsync("event"));
        results.Verify(repo => repo.AddWineResultAsync(It.IsAny<WineResult>()), Times.Never);
    }

    [Fact]
    public async Task EventEditing_PreservesPublicationAndIsAudited()
    {
        activeEvent.ResultsPublished = true;
        var edits = new Event { EventId = "event", Name = "Updated event", ResultsPublished = false };
        var service = new WineEventService(events.Object, Access("Admin"), mutations.Object);
        await service.UpdateEventAsync(edits);
        edits.ResultsPublished.ShouldBeTrue();
        mutations.Verify(store => store.CommitAsync(WineMongoDbContext.EventsCollection, "event", 0,
            activeEvent, It.Is<Event>(evt => evt.ResultsPublished && evt.Name == "Updated event"),
            SecurityTestIdentity.UserId, "EventUpdated"), Times.Once);
    }

    [Fact]
    public async Task DuplicateFirstSaves_UseSameMongoId()
    {
        var first = NewRating();
        var second = NewRating();
        await Judging().AddWineRatingAsync(first);
        await Judging().AddWineRatingAsync(second);
        first.WineRatingId.ShouldBe(second.WineRatingId);
    }

    [Fact]
    public async Task ExistingLegacyScore_PreventsDuplicateCreation()
    {
        var existing = NewRating();
        existing.JudgeId = SecurityTestIdentity.UserId;
        ratings.Setup(repo => repo.GetAllWineRatingsAsync()).ReturnsAsync(new List<WineRating> { existing });
        await Should.ThrowAsync<InvalidOperationException>(() => Judging().AddWineRatingAsync(NewRating()));
        mutations.VerifyNoOtherCalls();
    }
}