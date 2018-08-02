#if !NETSTANDARD2_0 && !NETCOREAPP2_0
using IdGen.Configuration;
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;

namespace IdGen
{
    /// <summary>
    /// Generates Id's inspired by Twitter's (late) Snowflake project.
    /// </summary>
    public class IdGenerator : IIdGenerator<long>
    {
        /// <summary>
        /// Returns the default epoch.
        /// </summary>
        public static readonly DateTime DefaultEpoch = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly ITimeSource defaulttimesource = new DefaultTimeSource(DefaultEpoch);
        private static readonly ConcurrentDictionary<string, IdGenerator> _namedgenerators = new ConcurrentDictionary<string, IdGenerator>();

        private int _sequence = 0;
        private long _lastgen = -1;
        private readonly long _generatorId;

        private readonly long MASK_SEQUENCE;
        private readonly long MASK_TIME;
        private readonly long MASK_GENERATOR;

        private readonly int SHIFT_TIME;
        private readonly int SHIFT_GENERATOR;


        // Object to lock() on while generating Id's
        private readonly object _genlock = new object();

        /// <summary>
        /// Gets the Id of the generator.
        /// </summary>
        public int Id { get { return (int)_generatorId; } }

        /// <summary>
        /// Gets the epoch for the <see cref="IdGenerator"/>.
        /// </summary>
        public DateTimeOffset Epoch { get { return TimeSource.Epoch; } }

        /// <summary>
        /// Gets the <see cref="MaskConfig"/> for the <see cref="IdGenerator"/>.
        /// </summary>
        public MaskConfig MaskConfig { get; private set; }

        /// <summary>
        /// Gets the <see cref="ITimeSource"/> for the <see cref="IdGenerator"/>.
        /// </summary>
        public ITimeSource TimeSource { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class, 2015-01-01 0:00:00Z is used as default 
        /// epoch and the <see cref="P:IdGen.MaskConfig.Default"/> value is used for the <see cref="MaskConfig"/>. The
        /// <see cref="DefaultTimeSource"/> is used to retrieve timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when GeneratorId exceeds maximum value.</exception>
        public IdGenerator(int generatorId)
            : this(generatorId, DefaultEpoch) { }

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
        public IdGenerator(int generatorId, DateTimeOffset epoch)
            : this(generatorId, epoch, MaskConfig.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.  The <see cref="DefaultTimeSource"/> is
        /// used to retrieve timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <exception cref="ArgumentNullException">Thrown when maskConfig is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when maskConfig defines a non-63 bit bitmask.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when GeneratorId or Sequence masks are >31 bit, GeneratorId exceeds maximum value or epoch in future.
        /// </exception>
        public IdGenerator(int generatorId, MaskConfig maskConfig)
            : this(generatorId, maskConfig, new DefaultTimeSource(DefaultEpoch)) { }

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
        public IdGenerator(int generatorId, DateTimeOffset epoch, MaskConfig maskConfig)
            : this(generatorId, maskConfig, new DefaultTimeSource(epoch)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="timeSource">The time-source to use when acquiring time data.</param>
        /// <exception cref="ArgumentNullException">Thrown when either maskConfig or timeSource is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when maskConfig defines a non-63 bit bitmask.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when GeneratorId or Sequence masks are >31 bit, GeneratorId exceeds maximum value or epoch in future.
        /// </exception>
        public IdGenerator(int generatorId, ITimeSource timeSource)
            : this(generatorId, MaskConfig.Default, timeSource) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <param name="timeSource">The time-source to use when acquiring time data.</param>
        /// <exception cref="ArgumentNullException">Thrown when either maskConfig or timeSource is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when maskConfig defines a non-63 bit bitmask.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when GeneratorId or Sequence masks are >31 bit, GeneratorId exceeds maximum value or epoch in future.
        /// </exception>
        public IdGenerator(int generatorId, MaskConfig maskConfig, ITimeSource timeSource)
        {
            if (maskConfig == null)
                throw new ArgumentNullException("maskConfig");

#pragma warning disable IDE0016
            if (timeSource == null)
                throw new ArgumentNullException("timeSource");
#pragma warning restore IDE0016

            if (maskConfig.TotalBits != 63)
                throw new InvalidOperationException("Number of bits used to generate Id's is not equal to 63");

            if (maskConfig.GeneratorIdBits > 31)
                throw new ArgumentOutOfRangeException("GeneratorId cannot have more than 31 bits");

            if (maskConfig.SequenceBits > 31)
                throw new ArgumentOutOfRangeException("Sequence cannot have more than 31 bits");

            // Precalculate some values
            MASK_TIME = GetMask(maskConfig.TimestampBits);
            MASK_GENERATOR = GetMask(maskConfig.GeneratorIdBits);
            MASK_SEQUENCE = GetMask(maskConfig.SequenceBits);

            if (generatorId < 0 || generatorId > MASK_GENERATOR)
                throw new ArgumentOutOfRangeException($"GeneratorId must be between 0 and {MASK_GENERATOR} (inclusive).");

            SHIFT_TIME = maskConfig.GeneratorIdBits + maskConfig.SequenceBits;
            SHIFT_GENERATOR = maskConfig.SequenceBits;

            // Store instance specific values
            MaskConfig = maskConfig;
            TimeSource = timeSource;
            _generatorId = generatorId;
        }

        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id based on the <see cref="IdGenerator"/>'s epoch, generatorid and sequence.</returns>
        /// <exception cref="InvalidSystemClockException">Thrown when clock going backwards is detected.</exception>
        /// <exception cref="SequenceOverflowException">Thrown when sequence overflows.</exception>
        public long CreateId()
        {
            lock (_genlock)
            {
                // Determine "timeslot" and make sure it's >= last timeslot (if any)
                var ticks = GetTicks();
                var timestamp = ticks & MASK_TIME;
                if (timestamp < _lastgen || ticks < 0)
                    throw new InvalidSystemClockException($"Clock moved backwards or wrapped around. Refusing to generate id for {_lastgen - timestamp} ticks");

                // If we're in the same "timeslot" as previous time we generated an Id, up the sequence number
                if (timestamp == _lastgen)
                {
                    if (_sequence < MASK_SEQUENCE)
                        _sequence++;
                    else
                        throw new SequenceOverflowException("Sequence overflow. Refusing to generate id for rest of tick");
                }
                else // If we're in a new(er) "timeslot", so we can reset the sequence and store the new(er) "timeslot"
                {
                    _sequence = 0;
                    _lastgen = timestamp;
                }

                unchecked
                {
                    // Build id by shifting all bits into their place
                    return (timestamp << SHIFT_TIME)
                        + (_generatorId << SHIFT_GENERATOR)
                        + _sequence;
                }
            }
        }

        /// <summary>
        /// Returns information about an Id such as the sequence number, generator id and date/time the Id was generated
        /// based on the current mask config of the generator.
        /// </summary>
        /// <param name="id">The Id to extract information from.</param>
        /// <returns>Returns an <see cref="ID" /> that contains information about the 'decoded' Id.</returns>
        /// <remarks>
        /// IMPORTANT: note that this method relies on the mask config and timesource; if the id was generated with a 
        /// diffferent mask config and/or timesource than the current one the 'decoded' ID will NOT contain correct 
        /// information.
        /// </remarks>
        public ID FromId(long id)
        {
            // Deconstruct Id by unshifting the bits into the proper parts
            return ID.Create(
                (int)(id & MASK_SEQUENCE),
                (int)((id >> SHIFT_GENERATOR) & MASK_GENERATOR),
                TimeSource.Epoch.Add(TimeSpan.FromTicks(((id >> SHIFT_TIME) & MASK_TIME) * TimeSource.TickDuration.Ticks))
            );
        }

#if !NETSTANDARD2_0 && !NETCOREAPP2_0
        /// <summary>
        /// Returns an instance of an <see cref="IdGenerator"/> based on the values in the corresponding idGenerator
        /// element in the idGenSection of the configuration file. The <see cref="DefaultTimeSource"/> is used to
        /// retrieve timestamp information.
        /// </summary>
        /// <param name="name">The name of the <see cref="IdGenerator"/> in the idGenSection.</param>
        /// <returns>An instance of an <see cref="IdGenerator"/> based on the values in the corresponding idGenerator
        /// element in the idGenSection of the configuration file.</returns>
        /// <remarks>
        /// When the <see cref="IdGenerator"/> doesn't exist it is created; any consequent calls to this method with
        /// the same name will return the same instance.
        /// </remarks>
        public static IdGenerator GetFromConfig(string name)
        {
            var result = _namedgenerators.GetOrAdd(name, (n) =>
            {
                var idgenerators = (ConfigurationManager.GetSection(IdGeneratorsSection.SectionName) as IdGeneratorsSection).IdGenerators;
                var idgen = idgenerators.OfType<IdGeneratorElement>().FirstOrDefault(e => e.Name.Equals(n));
                if (idgen != null)
                {
                    var ts = idgen.TickDuration == TimeSpan.Zero ? defaulttimesource : new DefaultTimeSource(idgen.Epoch, idgen.TickDuration);
                    return new IdGenerator(idgen.Id, new MaskConfig(idgen.TimestampBits, idgen.GeneratorIdBits, idgen.SequenceBits), ts);
                }

                throw new KeyNotFoundException();
            });

            return result;
        }
#endif

        /// <summary>
        /// Gets the number of ticks since the <see cref="ITimeSource"/>'s epoch.
        /// </summary>
        /// <returns>Returns the number of ticks since the <see cref="ITimeSource"/>'s epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetTicks()
        {
            return TimeSource.GetTicks();
        }

        /// <summary>
        /// Returns a bitmask masking out the desired number of bits; a bitmask of 2 returns 000...000011, a bitmask of
        /// 5 returns 000...011111.
        /// </summary>
        /// <param name="bits">The number of bits to mask.</param>
        /// <returns>Returns the desired bitmask.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                yield return CreateId();
        }

        /// <summary>
        /// Returns an enumerator that iterates over Id's.
        /// </summary>
        /// <returns>An <see cref="IEnumerator&lt;T&gt;"/> object that can be used to iterate over Id's.</returns>
        public IEnumerator<long> GetEnumerator()
        {
            return IdStream().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates over Id's.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate over Id's.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}