using IdGen;
using System;
using System.Threading;

namespace IdGenTests
{
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

        private int _autoincrementAfterCallsNum;
        private int _callsRemaining = -1;

        public int AutoincrementAfterCallsNum
        {
            set
            {
                _autoincrementAfterCallsNum = value;
                _callsRemaining = value;
            }
        }

        public long GetTicks()
        {
            if (_autoincrementAfterCallsNum > 0)
            {
                if (Interlocked.Decrement(ref _callsRemaining) == 0)
                {
                    _callsRemaining = _autoincrementAfterCallsNum;
                    NextTick();
                }
            }
            
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
