namespace WineApp.Tests;

/// <summary>
/// A <see cref="TimeProvider"/> implementation that always returns a fixed instant,
/// making time-dependent logic fully deterministic in unit tests.
/// </summary>
public sealed class FrozenTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _frozenUtc;

    public FrozenTimeProvider(DateTimeOffset frozenUtc)
    {
        _frozenUtc = frozenUtc;
    }

    /// <summary>Creates a provider frozen at the given UTC date/time.</summary>
    public static FrozenTimeProvider At(int year, int month, int day, int hour = 0, int minute = 0)
        => new(new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.Zero));

    public override DateTimeOffset GetUtcNow() => _frozenUtc;
}
