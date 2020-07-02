using System;
using System.Threading;

namespace IdGenTests.Mocks
{
    public class MockAutoIncrementingIntervalTimeSource : MockTimeSource
    {
        private readonly int _incrementEvery;
        private int _count;

        public MockAutoIncrementingIntervalTimeSource(int incrementEvery, long? current = null, TimeSpan? tickDuration = null, DateTimeOffset? epoch = null)
            : base(current ?? 0, tickDuration ?? TimeSpan.FromMilliseconds(1), epoch ?? DateTimeOffset.MinValue)
        {
            _incrementEvery = incrementEvery;
            _count = 0;
        }

        public override long GetTicks()
        {
            if (_count == _incrementEvery)
            {
                NextTick();
                _count = 0;
            }
            Interlocked.Increment(ref _count);

            return base.GetTicks();
        }
    }
}
