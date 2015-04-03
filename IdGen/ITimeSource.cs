using System;

namespace IdGen
{
    /// <summary>
    /// Provides the interface for timesources that provide time information to <see cref="IdGenerator"/>s.
    /// </summary>
    public interface ITimeSource
    {
        /// <summary>
        /// Returns a <see cref="DateTime"/> to be used by an <see cref="IdGenerator"/> when creating an Id.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> to be used by an <see cref="IdGenerator"/> when creating an Id.</returns>
        DateTime GetTime();
    }
}
