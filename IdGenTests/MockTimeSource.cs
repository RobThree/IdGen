using IdGen;
using System;
using System.Threading;

namespace IdGenTests
{
    public class MockTimeSource : ITimeSource
    {
        private long _current;

        private TimeSpan _tickduration;

        public MockTimeSource()
            : this(0) { }

        public MockTimeSource(long current)
            : this(current, TimeSpan.FromMilliseconds(1)) { }

        public MockTimeSource(TimeSpan tickDuration)
            : this(0, tickDuration) { }

        public MockTimeSource(long current, TimeSpan tickDuration)
        {
            _current = current;
            _tickduration = tickDuration;
        }

        public DateTimeOffset Epoch
        {
            get { return DateTimeOffset.MinValue; }
        }

        public TimeSpan TickDuration
        {
            get { return _tickduration; }
        }

        public long GetTicks()
        {
            return _current;
        }

        public void NextTick()
        {
            Interlocked.Increment(ref _current);
        }

        public void PreviousTick()
        {
            Interlocked.Decrement(ref _current);
        }
    }
}
