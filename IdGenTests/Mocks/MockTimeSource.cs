using IdGen;
using System;
using System.Threading;

namespace IdGenTests.Mocks;

public class MockTimeSource(long current, TimeSpan tickDuration, DateTimeOffset epoch) : ITimeSource
{
    public MockTimeSource()
        : this(0) { }

    public DateTimeOffset Epoch { get; private set; } = epoch;

    public TimeSpan TickDuration { get; } = tickDuration;

    public MockTimeSource(long current)
        : this(current, TimeSpan.FromMilliseconds(1), DateTimeOffset.MinValue) { }

    public MockTimeSource(TimeSpan tickDuration)
        : this(0, tickDuration, DateTimeOffset.MinValue) { }

    public virtual long GetTicks() => current;

    public void NextTick() => Interlocked.Increment(ref current);

    public void PreviousTick() => Interlocked.Decrement(ref current);
}
