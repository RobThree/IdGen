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
        //Based on / inspired by http://www.getcodesamples.com/src/B4BE9F4C/37A10013


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);


        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);


        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();


        private delegate ulong GetTickCount64Delegate();
        private static readonly GetTickCount64Delegate _tickcountdelegate = AcquireGetTickCount64Delegate();


        private static readonly DateTime _startupdate = DateTime.UtcNow - TimeSpan.FromMilliseconds(_tickcountdelegate());

        /// <summary>
        /// Returns a <see cref="DateTime"/> object that is set to the current date and time on this computer, expressed
        /// as the Coordinated Universal Time (UTC).
        /// </summary>
        /// <returns>
        /// A <see cref="DateTime"/> object that is set to the current date and time on this computer, expressed as the
        /// Coordinated Universal Time (UTC).
        /// </returns>
        /// <remarks>
        /// The resolution of this value depends on the system timer.
        /// </remarks>
        public DateTime GetTime()
        {
            return _startupdate.Add(TimeSpan.FromMilliseconds(_tickcountdelegate()));
        }

        /// <summary>
        /// Returns a <see cref="DateTime"/> object that is set to the current date and time on this computer, expressed
        /// as the Coordinated Universal Time (UTC).
        /// </summary>
        /// <returns>
        /// A <see cref="DateTime"/> object that is set to the current date and time on this computer, expressed as the
        /// Coordinated Universal Time (UTC).
        /// </returns>
        /// <remarks>
        /// The resolution of this value depends on the system timer.
        /// </remarks>
        DateTime ITimeSource.GetTime()
        {
            return this.GetTime();
        }

        /// <summary>
        ///    Returns a delegate that retrieves the number of milliseconds that have elapsed since the system was started.
        /// </summary>
        /// <remarks>
        /// <para>
        ///    This value is a monotonically increasing value, unlike DateTime.Now. DateTime.Now can appear to move 
        ///    backwards due to system time changes, which can cause bugs in programs that expect to be able to 
        ///    calculate elapsed time by subtracting DateTimes obtained from DateTime.Now.
        /// </para>
        /// <para>
        ///    IMPORTANT: On Vista+, this simply calls the native GetTickCount64. On XP, it emulates a 64-bit tick
        ///    counter. The emulation assumes that the calling program will call it <em>more than once every 49 days.</em>
        ///    If not, the results will be unpredictable.
        /// </para>
        /// </remarks>
        /// <returns>
        ///    A delegate that returns a 64-bit value that contains the number of milliseconds that have elapsed since
        ///    the system was started.
        /// </returns>
        private static GetTickCount64Delegate AcquireGetTickCount64Delegate()
        {
            IntPtr hKernel32 = LoadLibrary("kernel32.dll");
            if (IntPtr.Zero == hKernel32)
                throw new Win32Exception(); // Last win32 error will automatically be used.

            IntPtr ptrGetTickCount64 = GetProcAddress(hKernel32, "GetTickCount64");
            if (IntPtr.Zero != ptrGetTickCount64)
            {
                return (GetTickCount64Delegate)Marshal.GetDelegateForFunctionPointer(ptrGetTickCount64, typeof(GetTickCount64Delegate));
            }
            else
            {
                // We must be on XP.
                FreeLibrary(hKernel32);
                return EmulateGetTickCount64;
            }
        }


        private static uint sm_tickCountHi;
        private static uint sm_lastTickCountLo;

        // N.B. Must be called more than once every 49.7 days, else you get the wrong answer. But in no case will time
        // appear to move backwards.
        private static ulong EmulateGetTickCount64()
        {
            uint curTickCount = GetTickCount();
            if (curTickCount < sm_lastTickCountLo)
            {
                sm_tickCountHi++;
            }
            sm_lastTickCountLo = curTickCount;

            return (((ulong)sm_tickCountHi) << 32) + ((ulong)curTickCount);
        }
    }
}
