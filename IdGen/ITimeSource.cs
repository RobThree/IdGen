using System;

namespace IdGen
{
    /// <summary>
    /// Provides the interface for timesources that provide time information to <see cref="IdGenerator"/>s.
    /// </summary>
    public interface ITimeSource
    {
        /// <summary>
        /// Returns the duration of a single tick.
        /// </summary>
        /// <remarks>
        /// It's up to the <see cref="ITimeSource"/> to define what a 'tick' is; it may be nanoseconds, milliseconds,
        /// seconds or even days or years.
        /// </remarks>
        TimeSpan TickDuration { get; }

        /// <summary>
        /// Returns the current number of ticks for the <see cref="ITimeSource"/>.
        /// </summary>
        /// <returns>The current number of ticks to be used by an <see cref="IdGenerator"/> when creating an Id.</returns>
        /// <remarks>
        /// It's up to the <see cref="ITimeSource"/> to define what a 'tick' is; it may be nanoseconds, milliseconds,
        /// seconds or even days or years.
        /// </remarks>
        long GetTicks();
    }
}