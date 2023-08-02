using IdGen;
using System;
using System.Threading;

namespace IdGenTests.Mocks;

public class MockTimeSource : ITimeSource
{
    private long _current;

    public MockTimeSource()
        : this(0) { }

    public DateTimeOffset Epoch { get; private set; }

    public TimeSpan TickDuration { get; }

    public MockTimeSource(long current)
        : this(current, TimeSpan.FromMilliseconds(1), DateTimeOffset.MinValue) { }

    public MockTimeSource(TimeSpan tickDuration)
        : this(0, tickDuration, DateTimeOffset.MinValue) { }

    public MockTimeSource(long current, TimeSpan tickDuration, DateTimeOffset epoch)
    {
        _current = current;
        TickDuration = tickDuration;
        Epoch = epoch;
    }

    public virtual long GetTicks() => _current;

    public void NextTick() => Interlocked.Increment(ref _current);

    public void PreviousTick() => Interlocked.Decrement(ref _current);
}
