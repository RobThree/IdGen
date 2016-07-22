using IdGen.Configuration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
        private static readonly ITimeSource defaulttimesource = new DefaultTimeSource(defaultepoch);
        private static readonly ConcurrentDictionary<string, IdGenerator> _namedgenerators = new ConcurrentDictionary<string, IdGenerator>();

        private int _sequence = 0;
        private long _lastgen = -1;

        private readonly ITimeSource _timesource;
        private readonly DateTimeOffset _epoch;
        private readonly MaskConfig _maskconfig;
        private readonly long _generatorId;

        private readonly long MASK_SEQUENCE;
        private readonly long MASK_TIME;
        private readonly long MASK_GENERATOR;

        private readonly int SHIFT_TIME;
        private readonly int SHIFT_GENERATOR;


        // Object to lock() on while generating Id's
        private object genlock = new object();

        /// <summary>
        /// Gets the Id of the generator.
        /// </summary>
        public int Id { get { return (int)_generatorId; } }

        /// <summary>
        /// Gets the epoch for the <see cref="IdGenerator"/>.
        /// </summary>
        public DateTimeOffset Epoch { get { return _epoch; } }

        /// <summary>
        /// Gets the <see cref="MaskConfig"/> for the <see cref="IdGenerator"/>.
        /// </summary>
        public MaskConfig MaskConfig { get { return _maskconfig; } }

        /// <summary>
        /// Gets the <see cref="ITimeSource"/> for the <see cref="IdGenerator"/>.
        /// </summary>
        public ITimeSource TimeSource { get { return _timesource; } }

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
        /// Initializes a new instance of the <see cref="IdGenerator"/> class, 2015-01-01 0:00:00Z is used as default 
        /// epoch and the <see cref="P:IdGen.MaskConfig.Default"/> value is used for the <see cref="MaskConfig"/>. The
        /// <see cref="DefaultTimeSource"/> is used to retrieve timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="timeSource">The time-source to use when acquiring time data.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when GeneratorId exceeds maximum value.</exception>
        public IdGenerator(int generatorId, ITimeSource timeSource)
            : this(generatorId, defaultepoch, timeSource) { }

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
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="maskConfig">The <see cref="MaskConfig"/> of the generator.</param>
        /// <exception cref="ArgumentNullException">Thrown when maskConfig is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when maskConfig defines a non-63 bit bitmask.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when GeneratorId or Sequence masks are >31 bit, GeneratorId exceeds maximum value or epoch in future.
        /// </exception>
        public IdGenerator(int generatorId, DateTimeOffset epoch, MaskConfig maskConfig)
            : this(generatorId, epoch, maskConfig, defaulttimesource) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.  The <see cref="DefaultTimeSource"/> is
        /// used to retrieve timestamp information.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="epoch">The Epoch of the generator.</param>
        /// <param name="timeSource">The time-source to use when acquiring time data.</param>
        /// <exception cref="ArgumentNullException">Thrown when maskConfig is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when maskConfig defines a non-63 bit bitmask.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when GeneratorId or Sequence masks are >31 bit, GeneratorId exceeds maximum value or epoch in future.
        /// </exception>
        public IdGenerator(int generatorId, DateTimeOffset epoch, ITimeSource timeSource)
            : this(generatorId, epoch, MaskConfig.Default, timeSource) { }

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
        public IdGenerator(int generatorId, DateTimeOffset epoch, MaskConfig maskConfig, ITimeSource timeSource)
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

            // Precalculate some values
            MASK_TIME = GetMask(maskConfig.TimestampBits);
            MASK_GENERATOR = GetMask(maskConfig.GeneratorIdBits);
            MASK_SEQUENCE = GetMask(maskConfig.SequenceBits);

            if (generatorId < 0 || generatorId > MASK_GENERATOR)
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
                var ticks = this.GetTicks();
                var timestamp = ticks & MASK_TIME;
                if (timestamp < _lastgen || ticks < 0)
                    throw new InvalidSystemClockException(string.Format("Clock moved backwards or wrapped around. Refusing to generate id for {0} ticks", _lastgen - timestamp));

                if (timestamp == _lastgen)
                {
                    if (_sequence < MASK_SEQUENCE)
                        _sequence++;
                    else
                        throw new SequenceOverflowException("Sequence overflow. Refusing to generate id for rest of tick");
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
                    var ts = idgen.TickDuration == TimeSpan.Zero ? defaulttimesource : new DefaultTimeSource(DateTimeOffset.UtcNow, idgen.TickDuration);
                    return new IdGenerator(idgen.Id, idgen.Epoch, new MaskConfig(idgen.TimestampBits, idgen.GeneratorIdBits, idgen.SequenceBits), ts);
                }

                throw new KeyNotFoundException();
            });

            return result;
        }

        /// <summary>
        /// Gets the number of ticks since the <see cref="ITimeSource"/>'s epoch.
        /// </summary>
        /// <returns>Returns the number of ticks since the <see cref="ITimeSource"/>'s epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetTicks()
        {
            return _timesource.GetTicks();
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