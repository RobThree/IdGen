using System;

namespace IdGen
{
    /// <summary>
    /// Provides time data to an <see cref="IdGenerator"/>.
    /// </summary>
    /// <remarks>
    /// Unless specified the default duration of a tick for a <see cref="DefaultTimeSource"/> is 1 millisecond.
    /// </remarks>
    public class DefaultTimeSource : StopwatchTimeSource
    {
        /// <summary>
        /// Initializes a new <see cref="DefaultTimeSource"/> object.
        /// </summary>
        /// <param name="epoch">The epoch to use as an offset from now.</param>
        /// <remarks>The default tickduration is 1 millisecond.</remarks>
        public DefaultTimeSource(DateTimeOffset epoch)
            : this(epoch, TimeSpan.FromMilliseconds(1)) { }

        /// <summary>
        /// Initializes a new <see cref="DefaultTimeSource"/> object.
        /// </summary>
        /// <param name="epoch">The epoch to use as an offset from now,</param>
        /// <param name="tickDuration">The duration of a tick for this timesource.</param>
        public DefaultTimeSource(DateTimeOffset epoch, TimeSpan tickDuration)
            : base(epoch, tickDuration) { }

        /// <summary>
        /// Returns the current number of ticks for the <see cref="DefaultTimeSource"/>.
        /// </summary>
        /// <returns>The current number of ticks to be used by an <see cref="IdGenerator"/> when creating an Id.</returns>
        /// <remarks>
        /// Note that a 'tick' is a period defined by the timesource; this may be any valid <see cref="TimeSpan"/>; be
        /// it a millisecond, an hour, 2.5 seconds or any other value.
        /// </remarks>
        public override long GetTicks()
        {
            return (this.Offset.Ticks + this.Elapsed.Ticks) / this.TickDuration.Ticks;
        }
    }
}
