using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace WineApp.Tests.Services;

public class AuditRetentionTests
{
    private readonly Mock<IAuditRetentionStore> store = new();
    private readonly Mock<ISecurityMutationStore> mutations = new();
    private readonly TimeProvider clock = FrozenTimeProvider.At(2026, 10, 23);
    private readonly Event current = new()
    {
        Year = 2024, IsActive = false, EndedAtUtc = new DateTime(2024, 10, 23, 0, 0, 0, DateTimeKind.Utc)
    };

    public AuditRetentionTests()
    {
        store.Setup(value => value.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([current]);
        store.Setup(value => value.GetEventAsync(current.EventId, It.IsAny<CancellationToken>())).ReturnsAsync(current);
        store.Setup(value => value.WithEventLockAsync(It.IsAny<string>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((string eventId, Func<Task> operation, CancellationToken token) => operation());
    }

    private AuditRetentionService CreateSut(string role = "Admin") => new(
        SecurityTestIdentity.Create(null, role.Length == 0 ? [] : [role]), store.Object, mutations.Object,
        clock, NullLogger<AuditRetentionService>.Instance);

    [Theory]
    [InlineData(2024, 10, 22, true)]
    [InlineData(2024, 10, 23, true)]
    [InlineData(2024, 10, 24, false)]
    [InlineData(2026, 10, 23, false)]
    [InlineData(2027, 10, 23, false)]
    public void Policy_UsesTwentyFourCalendarMonths(int year, int month, int day, bool expected)
    {
        var evt = new Event { IsActive = false, EndedAtUtc = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc) };
        AuditRetentionPolicy.CanPurge(evt, FrozenTimeProvider.At(2026, 10, 23).GetUtcNow().UtcDateTime).ShouldBe(expected);
    }

    [Fact]
    public void Policy_HandlesLeapDayEndDate()
    {
        var evt = new Event { IsActive = false, EndedAtUtc = new DateTime(2024, 2, 29, 0, 0, 0, DateTimeKind.Utc) };
        AuditRetentionPolicy.CanPurge(evt, FrozenTimeProvider.At(2026, 2, 28).GetUtcNow().UtcDateTime).ShouldBeTrue();
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "DISPUTE-EXAMPLE")]
    public void Policy_PreservesActiveOrHeldEvents(bool active, string? dispute)
    {
        var evt = new Event
        {
            IsActive = active, EndedAtUtc = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AuditDisputeReference = dispute
        };
        AuditRetentionPolicy.CanPurge(evt, FrozenTimeProvider.At(2026, 10, 23).GetUtcNow().UtcDateTime).ShouldBeFalse();
    }

    [Fact]
    public void Policy_LegacyEventWithoutEndDateDoesNotExpire()
    {
        AuditRetentionPolicy.CanPurge(new Event { IsActive = false, Year = 2010 },
            FrozenTimeProvider.At(2026, 10, 23).GetUtcNow().UtcDateTime).ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("WineProducer")]
    [InlineData("Judge")]
    [InlineData("Viewer")]
    public async Task NonAdmins_CannotViewOrChangeRetention(string role)
    {
        var service = CreateSut(role);
        await Should.ThrowAsync<UnauthorizedAccessException>(() => service.GetEventsAsync());
        await Should.ThrowAsync<UnauthorizedAccessException>(() => service.RunCleanupAsync());
        await Should.ThrowAsync<UnauthorizedAccessException>(() => service.RecordEventEndAsync(current.EventId, clock.GetUtcNow().UtcDateTime));
        await Should.ThrowAsync<UnauthorizedAccessException>(() => service.SetDisputeHoldAsync(current.EventId, "EXAMPLE-CASE"));
        store.VerifyNoOtherCalls();
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Admin_CanRecordEndWithAudit()
    {
        current.IsActive = true;
        current.EndedAtUtc = null;
        await CreateSut().RecordEventEndAsync(current.EventId, clock.GetUtcNow().UtcDateTime);
        mutations.Verify(value => value.CommitAsync(WineMongoDbContext.EventsCollection, current.EventId, 0, current,
            It.Is<Event>(evt => !evt.IsActive && evt.EndedAtUtc == clock.GetUtcNow().UtcDateTime),
            SecurityTestIdentity.UserId, "EventEndRecorded"), Times.Once);
        current.EndedAtUtc.ShouldBeNull();
    }

    [Fact]
    public async Task FutureEventEnd_IsRejected()
    {
        await Should.ThrowAsync<ArgumentException>(() => CreateSut().RecordEventEndAsync(current.EventId, clock.GetUtcNow().UtcDateTime.AddDays(1)));
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task NonUtcEventEnd_IsRejected()
    {
        await Should.ThrowAsync<ArgumentException>(() => CreateSut().RecordEventEndAsync(current.EventId, new DateTime(2024, 1, 1)));
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EventEndBeforeEventYear_IsRejected()
    {
        await Should.ThrowAsync<ArgumentException>(() => CreateSut().RecordEventEndAsync(current.EventId,
            new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DocumentedDisputeHold_IsAudited()
    {
        await CreateSut().SetDisputeHoldAsync(current.EventId, "  EXAMPLE-CASE  ");
        mutations.Verify(value => value.CommitAsync(WineMongoDbContext.EventsCollection, current.EventId, 0, current,
            It.Is<Event>(evt => evt.AuditDisputeReference == "EXAMPLE-CASE"),
            SecurityTestIdentity.UserId, "AuditDisputeHoldPlaced"), Times.Once);
    }

    [Fact]
    public async Task ReleasingDisputeHold_IsExplicitAndAudited()
    {
        current.AuditDisputeReference = "EXAMPLE-CASE";
        await CreateSut().SetDisputeHoldAsync(current.EventId, null);
        mutations.Verify(value => value.CommitAsync(WineMongoDbContext.EventsCollection, current.EventId, 0, current,
            It.Is<Event>(evt => evt.AuditDisputeReference == null),
            SecurityTestIdentity.UserId, "AuditDisputeHoldReleased"), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("EXAMPLE\nCASE")]
    public async Task InvalidDisputeReference_IsRejected(string reference)
    {
        await Should.ThrowAsync<ArgumentException>(() => CreateSut().SetDisputeHoldAsync(current.EventId, reference));
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EligibleEvent_PurgesOnlyOldEntriesAndRecordsMaintenance()
    {
        var cutoff = new DateTime(2024, 10, 23, 0, 0, 0, DateTimeKind.Utc);
        store.Setup(value => value.RemoveExpiredEntriesAsync(current.EventId, cutoff, It.IsAny<CancellationToken>())).ReturnsAsync(3);
        (await CreateSut().RunCleanupAsync()).ShouldBe(3);
        mutations.Verify(value => value.CommitAsync(WineMongoDbContext.EventsCollection, current.EventId, 0,
            current, current, SecurityTestIdentity.UserId, "ExpiredAuditHistoryRemoved"), Times.Once);
    }

    [Fact]
    public async Task HoldAppliedAfterCandidateRead_IsRecheckedUnderLock()
    {
        var candidate = new Event { EventId = current.EventId, IsActive = false, EndedAtUtc = current.EndedAtUtc };
        store.Setup(value => value.GetEventsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([candidate]);
        current.AuditDisputeReference = "EXAMPLE-CASE";
        (await CreateSut().RunCleanupAsync()).ShouldBe(0);
        store.Verify(value => value.RemoveExpiredEntriesAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LegacyEventWithoutEndDate_IsNotPurged()
    {
        current.EndedAtUtc = null;
        (await CreateSut().RunCleanupAsync()).ShouldBe(0);
        store.Verify(value => value.WithEventLockAsync(It.IsAny<string>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BusyMaintenance_DoesNotAcknowledgeHoldAsSaved()
    {
        store.Setup(value => value.WithEventLockAsync(It.IsAny<string>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Busy"));
        await Should.ThrowAsync<InvalidOperationException>(() => CreateSut().SetDisputeHoldAsync(current.EventId, "EXAMPLE-CASE"));
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EventEdit_CannotRemoveHoldOrEndDate()
    {
        current.AuditDisputeReference = "EXAMPLE-CASE";
        var repository = new Mock<IEventRepository>();
        repository.Setup(value => value.GetEventByIdAsync(current.EventId)).ReturnsAsync(current);
        var service = new WineEventService(repository.Object, SecurityTestIdentity.Admin(), mutations.Object);
        var edits = new Event { EventId = current.EventId, IsActive = false };
        await service.UpdateEventAsync(edits);
        edits.AuditDisputeReference.ShouldBe("EXAMPLE-CASE");
        edits.EndedAtUtc.ShouldBe(current.EndedAtUtc);
    }

    [Fact]
    public async Task CompletedEvent_CannotBeReactivatedByOrdinaryEdit()
    {
        var repository = new Mock<IEventRepository>();
        repository.Setup(value => value.GetEventByIdAsync(current.EventId)).ReturnsAsync(current);
        var service = new WineEventService(repository.Object, SecurityTestIdentity.Admin(), mutations.Object);
        await Should.ThrowAsync<InvalidOperationException>(() => service.UpdateEventAsync(new Event { EventId = current.EventId, IsActive = true }));
        mutations.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReleasedHold_MakesExpiredHistoryEligibleAgain()
    {
        current.AuditDisputeReference = "EXAMPLE-CASE";
        (await CreateSut().RunCleanupAsync()).ShouldBe(0);
        current.AuditDisputeReference = null;
        store.Setup(value => value.RemoveExpiredEntriesAsync(current.EventId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        (await CreateSut().RunCleanupAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task FailedCleanupAudit_IsNotReportedAsSuccess()
    {
        store.Setup(value => value.RemoveExpiredEntriesAsync(current.EventId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mutations.Setup(value => value.CommitAsync(WineMongoDbContext.EventsCollection, current.EventId, 0,
            current, current, SecurityTestIdentity.UserId, "ExpiredAuditHistoryRemoved"))
            .ThrowsAsync(new InvalidOperationException("Simulated audit persistence failure"));
        await Should.ThrowAsync<InvalidOperationException>(() => CreateSut().RunCleanupAsync());
    }

    [Fact]
    public async Task BackgroundWorker_UsesSystemIdentityForStartupSweep()
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        mutations.Setup(value => value.CommitAsync(WineMongoDbContext.EventsCollection, current.EventId, 0,
            current, current, "system:audit-retention", "ExpiredAuditHistoryRemoved"))
            .Callback(() => completed.TrySetResult()).Returns(Task.CompletedTask);
        store.Setup(value => value.RemoveExpiredEntriesAsync(current.EventId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var provider = new Mock<IServiceProvider>();
        provider.Setup(value => value.GetService(typeof(AuditRetentionService))).Returns(CreateSut(""));
        var scope = new Mock<IServiceScope>();
        scope.SetupGet(value => value.ServiceProvider).Returns(provider.Object);
        var scopes = new Mock<IServiceScopeFactory>();
        scopes.Setup(value => value.CreateScope()).Returns(scope.Object);
        using var worker = new AuditRetentionWorker(scopes.Object, clock, NullLogger<AuditRetentionWorker>.Instance);
        await worker.StartAsync(CancellationToken.None);
        try { await completed.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
        finally { await worker.StopAsync(CancellationToken.None); }
        mutations.Verify(value => value.CommitAsync(WineMongoDbContext.EventsCollection, current.EventId, 0,
            current, current, "system:audit-retention", "ExpiredAuditHistoryRemoved"), Times.Once);
    }
}