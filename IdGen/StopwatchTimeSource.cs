using System;
using System.Diagnostics;

namespace IdGen
{
    /// <summary>
    /// Provides time data to an <see cref="IdGenerator"/>. This timesource uses a <see cref="Stopwatch"/> for timekeeping.
    /// </summary>
    public abstract class StopwatchTimeSource : ITimeSource
    {
        private static readonly Stopwatch _sw = Stopwatch.StartNew();

        /// <summary>
        /// Gets the elapsed time since this <see cref="ITimeSource"/> was initialized.
        /// </summary>
        protected TimeSpan Elapsed { get { return _sw.Elapsed; } }

        /// <summary>
        /// Gets the offset for this <see cref="ITimeSource"/> which is defined as the difference of it's creationdate
        /// and it's epoch which is specified in the object's constructor.
        /// </summary>
        protected TimeSpan Offset { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="StopwatchTimeSource"/> object.
        /// </summary>
        /// <param name="epoch">The epoch to use as an offset from now,</param>
        /// <param name="tickDuration">The duration of a single tick for this timesource.</param>
        public StopwatchTimeSource(DateTimeOffset epoch, TimeSpan tickDuration)
        {
            this.Offset = (DateTimeOffset.UtcNow - epoch);
            this.TickDuration = tickDuration;
        }

        /// <summary>
        /// Returns the duration of a single tick.
        /// </summary>
        public TimeSpan TickDuration { get; private set; }

        /// <summary>
        /// Returns the current number of ticks for the <see cref="DefaultTimeSource"/>.
        /// </summary>
        /// <returns>The current number of ticks to be used by an <see cref="IdGenerator"/> when creating an Id.</returns>
        public abstract long GetTicks();
    }
}
