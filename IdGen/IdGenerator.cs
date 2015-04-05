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
    public class IdGenerator : IIdGenerator<long>
    {
        private static readonly DateTime defaultepoch = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly ITimeSource defaulttimesource = new DefaultTimeSource();

        private int _sequence = 0;
        private long _lastgen = -1;

        private readonly DateTime _epoch;
        private readonly MaskConfig _maskconfig;
        private readonly long _generatorId;

        private readonly long MASK_SEQUENCE;
        private readonly long MASK_TIME;
        private readonly long MASK_GENERATOR;

        private readonly int SHIFT_TIME;
        private readonly int SHIFT_GENERATOR;

        private readonly ITimeSource _timesource;

        // Object to lock() on while generating Id's
        private object genlock = new object();

        /// <summary>
        /// Gets the Id of the generator.
        /// </summary>
        public int Id { get { return (int)_generatorId; } }

        /// <summary>
        /// Gets the epoch for the <see cref="IdGenerator"/>.
        /// </summary>
        public DateTime Epoch { get { return _epoch; } }

        /// <summary>
        /// Gets the <see cref="MaskConfig"/> for the <see cref="IdGenerator"/>.
        /// </summary>
        public MaskConfig MaskConfig { get { return _maskconfig; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class, 2015-01-01 0:00:00Z is used as default 
        /// epoch and the <see cref="P:IdGen.MaskConfig.Default"/> value is used for the <see cref="MaskConfig"/>. The
        /// <see cref="DefaultTimeSource"/> is used to retrieve timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when GeneratorId exceeds maximum value.</exception>
        public IdGenerator(int generatorId)
            : this(generatorId, defaultepoch) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class. The <see cref="P:IdGen.MaskConfig.Default"/> 
        /// value is used for the <see cref="MaskConfig"/>.  The <see cref="DefaultTimeSource"/> is used to retrieve
        /// timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when GeneratorId exceeds maximum value or epoch in future.
        /// </exception>
        public IdGenerator(int generatorId, DateTime epoch)
            : this(generatorId, epoch, MaskConfig.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.  The <see cref="DefaultTimeSource"/> is
        /// used to retrieve timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <exception cref="ArgumentNullException">Thrown when maskConfig is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when maskConfig defines a non-63 bit bitmask.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when GeneratorId or Sequence masks are >31 bit, GeneratorId exceeds maximum value or epoch in future.
        /// </exception>
        public IdGenerator(int generatorId, DateTime epoch, MaskConfig maskConfig)
            : this(generatorId, epoch, maskConfig, defaulttimesource) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <param name="timeSource">The time-source to use when acquiring time data.</param>
        /// <exception cref="ArgumentNullException">Thrown when either maskConfig or timeSource is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when maskConfig defines a non-63 bit bitmask.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when GeneratorId or Sequence masks are >31 bit, GeneratorId exceeds maximum value or epoch in future.
        /// </exception>
        public IdGenerator(int generatorId, DateTime epoch, MaskConfig maskConfig, ITimeSource timeSource)
        {
            if (maskConfig == null)
                throw new ArgumentNullException("maskConfig");

            if (timeSource == null)
                throw new ArgumentNullException("timeSource");

            if (maskConfig.TotalBits != 63)
                throw new InvalidOperationException("Number of bits used to generate Id's is not equal to 63");

            if (maskConfig.GeneratorIdBits > 31)
                throw new ArgumentOutOfRangeException("GeneratorId cannot have more than 31 bits");

            if (maskConfig.SequenceBits > 31)
                throw new ArgumentOutOfRangeException("Sequence cannot have more than 31 bits");

            if (epoch > timeSource.GetTime())
                throw new ArgumentOutOfRangeException("Epoch in future");

            // Precalculate some values
            MASK_TIME = GetMask(maskConfig.TimestampBits);
            MASK_GENERATOR = GetMask(maskConfig.GeneratorIdBits);
            MASK_SEQUENCE = GetMask(maskConfig.SequenceBits);

            if (generatorId > MASK_GENERATOR)
                throw new ArgumentOutOfRangeException(string.Format("GeneratorId must be between 0 and {0} (inclusive).", MASK_GENERATOR));

            SHIFT_TIME = maskConfig.GeneratorIdBits + maskConfig.SequenceBits;
            SHIFT_GENERATOR = maskConfig.SequenceBits;

            // Store instance specific values
            _maskconfig = maskConfig;
            _timesource = timeSource;
            _epoch = epoch;
            _generatorId = generatorId;
        }

        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id based on the <see cref="IdGenerator"/>'s epoch, generatorid and sequence.</returns>
        /// <exception cref="InvalidSystemClockException">Thrown when clock going backwards is detected.</exception>
        /// <exception cref="SequenceOverflowException">Thrown when sequence overflows.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long CreateId()
        {
            lock (genlock)
            {
                var timestamp = this.GetTimestamp() & MASK_TIME;
                if (timestamp < _lastgen)
                    throw new InvalidSystemClockException(string.Format("Clock moved backwards or wrapped around. Refusing to generate id for {0} milliseconds", _lastgen - timestamp));

                if (timestamp == _lastgen)
                {
                    if (_sequence < MASK_SEQUENCE)
                        _sequence++;
                    else
                        throw new SequenceOverflowException("Sequence overflow. Refusing to generate id for rest of millisecond");
                }
                else
                {
                    _sequence = 0;
                    _lastgen = timestamp;
                }

                unchecked
                {
                    return (timestamp << SHIFT_TIME)
                        + (_generatorId << SHIFT_GENERATOR)         // GeneratorId is already masked, we only need to shift
                        + _sequence;
                }
            }
        }

        /// <summary>
        /// Returns a new instance of an <see cref="IdGenerator"/> based on the machine-name.
        /// </summary>
        /// <returns>A new instance of an <see cref="IdGenerator"/> based on the machine-name</returns>
        /// <remarks>
        /// Note: be very careful using this method; it is recommended to explicitly set an generatorId instead since
        /// a hash of the machinename could result in a collision (especially when the bitmask for the generator is
        /// very small) of the generator-id's across machines. Only use this in small setups (few hosts) and if you have
        /// no other choice. Prefer to specify generator id's via configuration file or other means instead.
        /// </remarks>
        public static IdGenerator GetMachineSpecificGenerator()
        {
            return GetMachineSpecificGenerator(defaultepoch);
        }

        /// <summary>
        /// Returns a new instance of an <see cref="IdGenerator"/> based on the machine-name.
        /// </summary>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <returns>A new instance of an <see cref="IdGenerator"/> based on the machine-name</returns>
        /// <remarks>
        /// Note: be very careful using this method; it is recommended to explicitly set an generatorId instead since
        /// a hash of the machinename could result in a collision (especially when the bitmask for the generator is
        /// very small) of the generator-id's across machines. Only use this in small setups (few hosts) and if you have
        /// no other choice. Prefer to specify generator id's via configuration file or other means instead.
        /// </remarks>
        public static IdGenerator GetMachineSpecificGenerator(DateTime epoch)
        {
            return GetMachineSpecificGenerator(epoch, MaskConfig.Default);
        }

        /// <summary>
        /// Returns a new instance of an <see cref="IdGenerator"/> based on the machine-name.
        /// </summary>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <returns>A new instance of an <see cref="IdGenerator"/> based on the machine-name</returns>
        /// <remarks>
        /// Note: be very careful using this method; it is recommended to explicitly set an generatorId instead since
        /// a hash of the machinename could result in a collision (especially when the bitmask for the generator is
        /// very small) of the generator-id's across machines. Only use this in small setups (few hosts) and if you have
        /// no other choice. Prefer to specify generator id's via configuration file or other means instead.
        /// </remarks>
        public static IdGenerator GetMachineSpecificGenerator(DateTime epoch, MaskConfig maskConfig)
        {
            return GetMachineSpecificGenerator(epoch, maskConfig, defaulttimesource);
        }

        /// <summary>
        /// Returns a new instance of an <see cref="IdGenerator"/> based on the machine-name.
        /// </summary>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <param name="timeSource">The time-source to use when acquiring time data.</param>
        /// <returns>A new instance of an <see cref="IdGenerator"/> based on the machine-name</returns>
        /// <remarks>
        /// Note: be very careful using this method; it is recommended to explicitly set an generatorId instead since
        /// a hash of the machinename could result in a collision (especially when the bitmask for the generator is
        /// very small) of the generator-id's across machines. Only use this in small setups (few hosts) and if you have
        /// no other choice. Prefer to specify generator id's via configuration file or other means instead.
        /// </remarks>
        public static IdGenerator GetMachineSpecificGenerator(DateTime epoch, MaskConfig maskConfig, ITimeSource timeSource)
        {
            return new IdGenerator(GetMachineHash() & maskConfig.GeneratorIdBits, epoch, maskConfig, timeSource);
        }

        /// <summary>
        /// Returns a new instance of an <see cref="IdGenerator"/> based on the (managed) thread this method is invoked on.
        /// </summary>
        /// <returns>A new instance of an <see cref="IdGenerator"/> based on the (managed) thread this method is invoked on.</returns>
        /// <remarks>
        /// Note: This method can be used when using several threads on a single machine to get thread-specific generators;
        /// if this method is used across machines there's a high probability of collisions in generator-id's. In that
        /// case prefer to explicitly set the generator id's via configuration file or other means instead.
        /// </remarks>
        public static IdGenerator GetThreadSpecificGenerator()
        {
            return GetThreadSpecificGenerator(defaultepoch);
        }

        /// <summary>
        /// Returns a new instance of an <see cref="IdGenerator"/> based on the (managed) thread this method is invoked on.
        /// </summary>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <returns>A new instance of an <see cref="IdGenerator"/> based on the (managed) thread this method is invoked on.</returns>
        /// <remarks>
        /// Note: This method can be used when using several threads on a single machine to get thread-specific generators;
        /// if this method is used across machines there's a high probability of collisions in generator-id's. In that
        /// case prefer to explicitly set the generator id's via configuration file or other means instead.
        /// </remarks>
        public static IdGenerator GetThreadSpecificGenerator(DateTime epoch)
        {
            return GetThreadSpecificGenerator(epoch, MaskConfig.Default);
        }

        /// <summary>
        /// Returns a new instance of an <see cref="IdGenerator"/> based on the (managed) thread this method is invoked on.
        /// </summary>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <returns>A new instance of an <see cref="IdGenerator"/> based on the (managed) thread this method is invoked on.</returns>
        /// <remarks>
        /// Note: This method can be used when using several threads on a single machine to get thread-specific generators;
        /// if this method is used across machines there's a high probability of collisions in generator-id's. In that
        /// case prefer to explicitly set the generator id's via configuration file or other means instead.
        /// </remarks>
        public static IdGenerator GetThreadSpecificGenerator(DateTime epoch, MaskConfig maskConfig)
        {
            return GetThreadSpecificGenerator(epoch, maskConfig, defaulttimesource);
        }

        /// <summary>
        /// Returns a new instance of an <see cref="IdGenerator"/> based on the (managed) thread this method is invoked on.
        /// </summary>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <param name="timeSource">The time-source to use when acquiring time data.</param>
        /// <returns>A new instance of an <see cref="IdGenerator"/> based on the (managed) thread this method is invoked on.</returns>
        /// <remarks>
        /// Note: This method can be used when using several threads on a single machine to get thread-specific generators;
        /// if this method is used across machines there's a high probability of collisions in generator-id's. In that
        /// case prefer to explicitly set the generator id's via configuration file or other means instead.
        /// </remarks>
        public static IdGenerator GetThreadSpecificGenerator(DateTime epoch, MaskConfig maskConfig, ITimeSource timeSource)
        {
            return new IdGenerator(GetThreadId() & maskConfig.GeneratorIdBits, epoch, maskConfig, timeSource);
        }

        /// <summary>
        /// Gets a unique identifier for the current managed thread.
        /// </summary>
        /// <returns>An integer that represents a unique identifier for this managed thread.</returns>
        private static int GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
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
        /// Gets the number of milliseconds since the <see cref="IdGenerator"/>'s epoch.
        /// </summary>
        /// <returns>Returns the number of milliseconds since the <see cref="IdGenerator"/>'s epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetTimestamp()
        {
            return (long)(_timesource.GetTime() - _epoch).TotalMilliseconds;
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