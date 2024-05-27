using System;
using System.Threading;

namespace IdGenTests.Mocks;

public class MockAutoIncrementingIntervalTimeSource(int incrementEvery, long? current = null, TimeSpan? tickDuration = null, DateTimeOffset? epoch = null)
    : MockTimeSource(current ?? 0, tickDuration ?? TimeSpan.FromMilliseconds(1), epoch ?? DateTimeOffset.MinValue)
{
    private int _count = 0;

    public override long GetTicks()
    {
        if (_count == incrementEvery)
        {
            NextTick();
            _count = 0;
        }
        Interlocked.Increment(ref _count);

        return base.GetTicks();
    }
}
