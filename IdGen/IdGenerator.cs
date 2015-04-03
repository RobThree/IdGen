using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IdGen
{
    /// <summary>
    /// Generates Id's inspired by Twitter's (late) Snowflake project.
    /// </summary>
    public class IdGenerator : IEnumerable<long>
    {
        private int _sequence = 0;
        private long _lastgen = -1;

        private readonly DateTime _epoch;
        private readonly int _generatorId;

        private readonly long MASK_SEQUENCE;
        private readonly long MASK_TIME;
        private readonly int SHIFT_TIME;
        private readonly int SHIFT_GENERATOR;

        private readonly ITimeSource _timesource;

        // Object to lock() on while generating Id's
        private object genlock = new object();

        /// <summary>
        /// Gets the Id of the generator.
        /// </summary>
        /// <remarks>
        /// The returned value MAY not be the generatorId passed into one of the <see cref="IdGenerator"/>'s 
        /// constructors; the Id will be masked by the <see cref="MaskConfig"/>'s
        /// <see cref="MaskConfig.GeneratorIdBits"/>-mask.
        /// </remarks>
        public int Id { get { return _generatorId; } }

        /// <summary>
        /// Gets the epoch for the <see cref="IdGenerator"/>.
        /// </summary>
        public DateTime Epoch { get { return _epoch; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class. A deterministic <see cref="Id"/> is
        /// automatically assigned based on the machinename, 2015-01-01 0:00:00Z is used as default epoch and the
        /// <see cref="MaskConfig.Default"/> value is used for the <see cref="MaskConfig"/>. The
        /// <see cref="DefaultTimeSource"/> is used to retrieve timestamp information.
        /// </summary>
        public IdGenerator()
            : this(GetMachineHash()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class, 2015-01-01 0:00:00Z is used as default 
        /// epoch and the <see cref="MaskConfig.Default"/> value is used for the <see cref="MaskConfig"/>. The
        /// <see cref="DefaultTimeSource"/> is used to retrieve timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        public IdGenerator(int generatorId)
            : this(generatorId, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class. A deterministic <see cref="Id"/> is
        /// automatically assigned based on the machinename and the <see cref="MaskConfig.Default"/> value is used for
        /// the <see cref="MaskConfig"/>.  The <see cref="DefaultTimeSource"/> is used to retrieve timestamp
        /// information.
        /// </summary>
        /// <param name="epoch">The Epoch of the generator.</param>
        public IdGenerator(DateTime epoch)
            : this(GetMachineHash(), epoch) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class. The <see cref="MaskConfig.Default"/> 
        /// value is used for the <see cref="MaskConfig"/>.  The <see cref="DefaultTimeSource"/> is used to retrieve
        /// timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="epoch">The Epoch of the generator.</param>
        public IdGenerator(int generatorId, DateTime epoch)
            : this(generatorId, epoch, MaskConfig.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.  The <see cref="DefaultTimeSource"/> is
        /// used to retrieve timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        public IdGenerator(int generatorId, DateTime epoch, MaskConfig maskConfig)
            : this(generatorId, epoch, maskConfig, new DefaultTimeSource()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <param name="timeSource">The time-source to use when acquiring time data.</param>
        public IdGenerator(int generatorId, DateTime epoch, MaskConfig maskConfig, ITimeSource timeSource)
        {
            if (maskConfig == null)
                throw new ArgumentNullException("maskConfig");

            if (timeSource == null)
                throw new ArgumentNullException("timeSource");

            if (maskConfig.TotalBits != 63)
                throw new InvalidOperationException("Number of bits used to generate ID's is not equal to 63");

            //TODO: Sanity-check mask-config for sane ranges + unittests...

            // Precalculate some values
            MASK_TIME = GetMask(maskConfig.TimestampBits);
            MASK_SEQUENCE = GetMask(maskConfig.SequenceBits);

            SHIFT_TIME = maskConfig.GeneratorIdBits + maskConfig.SequenceBits;
            SHIFT_GENERATOR = maskConfig.SequenceBits;

            // Store instance specific values
            _timesource = timeSource;
            _epoch = epoch;
            _generatorId = (int)(generatorId & GetMask(maskConfig.GeneratorIdBits));
        }

        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id based on the <see cref="IdGenerator"/>'s epoch, generatorid and sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long CreateId()
        {
            lock (genlock)
            {
                var timestamp = this.GetTimestamp();

                //TODO: Benchmark below method and commented-out method
                //=================
                if (timestamp == _lastgen)
                {
                    _sequence++;
                    if (_sequence > MASK_SEQUENCE)
                    {
                        //TODO: Do we actually want to wait? Or should we simply throw some "Can't generate Id" exception?
                        while (_lastgen == this.GetTimestamp())
                            Thread.Sleep(0);
                        _sequence = 0;
                    }
                }
                else
                {
                    _sequence = 0;
                    _lastgen = timestamp;
                }

                //while (timestamp == _lastgen && _sequence >= MASK_SEQUENCE)
                //{
                //    Thread.Sleep(0);
                //    timestamp = GetTimestamp();
                //}

                //_sequence = timestamp == _lastgen ? _sequence + 1 : 0;
                //_lastgen = timestamp;
                //=================

                unchecked
                {
                    return ((timestamp & MASK_TIME) << SHIFT_TIME)
                        + (_generatorId << SHIFT_GENERATOR)         // GeneratorId is already masked, we only need to shift
                        + (_sequence & MASK_SEQUENCE);
                }
            }
        }

        /// <summary>
        /// Gets the number of milliseconds since the <see cref="IdGenerator"/>'s epoch.
        /// </summary>
        /// <returns>Returns the number of milliseconds since the <see cref="IdGenerator"/>'s epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetTimestamp()
        {
            return (long)(_timesource.GetTime() - _epoch).TotalMilliseconds;
        }

        /// <summary>
        /// Gets a hashcode based on the <see cref="Environment.MachineName"/>.
        /// </summary>
        /// <returns>Returns a hashcode based on the <see cref="Environment.MachineName"/>.</returns>
        private static int GetMachineHash()
        {
            return Environment.MachineName.GetHashCode();
        }

        /// <summary>
        /// Returns a bitmask masking out the desired number of bits; a bitmask of 2 returns 000...000011, a bitmask of
        /// 5 returns 000...011111.
        /// </summary>
        /// <param name="bits">The number of bits to mask.</param>
        /// <returns>Returns the desired bitmask.</returns>
        private static long GetMask(byte bits)
        {
            return (1L << bits) - 1;
        }

        /// <summary>
        /// Returns a 'never ending' stream of Id's.
        /// </summary>
        /// <returns>A 'never ending' stream of Id's.</returns>
        private IEnumerable<long> IdStream()
        {
            while (true)
                yield return this.CreateId();
        }

        /// <summary>
        /// Returns an enumerator that iterates over Id's.
        /// </summary>
        /// <returns>An <see cref="IEnumerator&lt;T&gt;"/> object that can be used to iterate over Id's.</returns>
        public IEnumerator<long> GetEnumerator()
        {
            return this.IdStream().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates over Id's.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate over Id's.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
