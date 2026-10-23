namespace Atm.Application.Tests.Fakes;

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public static readonly DateTimeOffset Default = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public FixedTimeProvider() : this(Default)
    {
    }

    public override DateTimeOffset GetUtcNow() => now;
}
