using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace IdGen
{
    /// <summary>
    /// Provides time data to an <see cref="IdGenerator"/>. Uses the current date and time on this computer.
    /// </summary>
    public class DefaultTimeSource : ITimeSource
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long _frequency;
        private long _offset;
        private DateTime _start;

        public DefaultTimeSource()
        {
            QueryPerformanceFrequency(out _frequency);
            QueryPerformanceCounter(out _offset);
            _start = DateTime.UtcNow;
        }
        

        /// <summary>
        /// Returns a <see cref="DateTime"/> object that is (close to) the current date and time on this computer, expressed
        /// as the Coordinated Universal Time (UTC).
        /// </summary>
        /// <returns>
        /// A <see cref="DateTime"/> object that is (close to) the current date and time on this computer, expressed as the
        /// Coordinated Universal Time (UTC).
        /// </returns>
        /// <remarks>
        /// The resolution of this value depends on the system. It does *not* rely on the system- or wall-clock time but
        /// on QueryPerformanceCounter and *may* (and *will*) drift ahead of time over time.
        /// </remarks>
        public DateTime GetTime()
        {
            return _start.AddSeconds(GetSecondsSinceStart());
        }

        /// <summary>
        /// Returns a <see cref="DateTime"/> object that is (close to) the current date and time on this computer, expressed
        /// as the Coordinated Universal Time (UTC).
        /// </summary>
        /// <returns>
        /// A <see cref="DateTime"/> object that is (close to) the current date and time on this computer, expressed as the
        /// Coordinated Universal Time (UTC).
        /// </returns>
        /// <remarks>
        /// The resolution of this value depends on the system. It does *not* rely on the system- or wall-clock time but
        /// on QueryPerformanceCounter and *may* (and *will*) drift ahead of time over time.
        /// </remarks>
        DateTime ITimeSource.GetTime()
        {
            return this.GetTime();
        }

        private double GetSecondsSinceStart()
        {
            long t;
            QueryPerformanceCounter(out t);
            return (double)(t - _offset) / _frequency;
        }
    }
}
