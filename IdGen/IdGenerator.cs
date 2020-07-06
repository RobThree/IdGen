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
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class IdGenerator : IIdGenerator<long>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        private readonly long _generatorid;
        private int _sequence = 0;
        private long _lastgen = -1;

        private readonly long MASK_SEQUENCE;
        private readonly long MASK_TIME;
        private readonly long MASK_GENERATOR;

        private readonly int SHIFT_TIME;
        private readonly int SHIFT_GENERATOR;


        // Object to lock() on while generating Id's
        private readonly object _genlock = new object();

        /// <summary>
        /// Gets the <see cref="IdGeneratorOptions"/>.
        /// </summary>
        public IdGeneratorOptions Options { get; }


        /// <summary>
        /// Gets the Id of the generator.
        /// </summary>
        public int Id => (int)_generatorid;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        public IdGenerator(int generatorId)
            : this(generatorId, new IdGeneratorOptions()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdGenerator"/> class with the specified <see cref="IdGeneratorOptions"/>.
        /// </summary>
        /// <param name="generatorId">The Id of the generator.</param>
        /// <param name="options">The <see cref="IdGeneratorOptions"/> for the <see cref="IdGenerator"/></param>.
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public IdGenerator(int generatorId, IdGeneratorOptions options)
        {
            if (generatorId < 0)
                throw new ArgumentOutOfRangeException(nameof(generatorId), "GeneratorId must be larger than or equal to 0");
            _generatorid = generatorId;

            Options = options ?? throw new ArgumentNullException(nameof(options));

            var maxgeneratorid = 1 << Options.IdStructure.GeneratorIdBits;
            if (_generatorid >= maxgeneratorid)
                throw new ArgumentOutOfRangeException(nameof(generatorId), $"GeneratorId must be between 0 and {maxgeneratorid - 1}.");

            // Precalculate some values
            MASK_TIME = GetMask(options.IdStructure.TimestampBits);
            MASK_GENERATOR = GetMask(options.IdStructure.GeneratorIdBits);
            MASK_SEQUENCE = GetMask(options.IdStructure.SequenceBits);
            SHIFT_TIME = options.IdStructure.GeneratorIdBits + options.IdStructure.SequenceBits;
            SHIFT_GENERATOR = options.IdStructure.SequenceBits;
        }

        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id based on the <see cref="IdGenerator"/>'s epoch, generatorid and sequence.</returns>
        /// <exception cref="InvalidSystemClockException">Thrown when clock going backwards is detected.</exception>
        /// <exception cref="SequenceOverflowException">Thrown when sequence overflows.</exception>
        /// <remarks>Note that this method MAY throw an one of the documented exceptions.</remarks>
        public long CreateId()
        {
            var id = CreateIdImpl(out var ex);
            if (ex != null)
                throw ex;
            return id;
        }

        /// <summary>
        /// Attempts to a new Id. A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="id">
        /// When this method returns, contains the generated Id if the method succeeded. If the method failed, as
        /// indicated by the return value, no guarantees can be made about the id. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.
        /// </param>
        /// <returns>true if an Id was generated successfully; false otherwise.</returns>
        /// <remarks>This method will not throw exceptions but rather indicate success by the return value.</remarks>
        public bool TryCreateId(out long id)
        {
            id = CreateIdImpl(out var ex);
            return ex == null;
        }

        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <param name="exception">If any exceptions occur they will be returned in this argument.</param>
        /// <returns>
        /// Returns an Id based on the <see cref="IdGenerator"/>'s epoch, generatorid and sequence or
        /// a negative value when an exception occurred.
        /// </returns>
        /// <exception cref="InvalidSystemClockException">Thrown when clock going backwards is detected.</exception>
        /// <exception cref="SequenceOverflowException">Thrown when sequence overflows.</exception>
        private long CreateIdImpl(out Exception? exception)
        {
            lock (_genlock)
            {
                // Determine "timeslot" and make sure it's >= last timeslot (if any)
                var ticks = GetTicks();
                var timestamp = ticks & MASK_TIME;

                if (timestamp < _lastgen || ticks < 0)
                {
                    exception = new InvalidSystemClockException($"Clock moved backwards or wrapped around. Refusing to generate id for {_lastgen - timestamp} ticks");
                    return -1;
                }

                // If we're in the same "timeslot" as previous time we generated an Id, up the sequence number
                if (timestamp == _lastgen)
                {
                    if (_sequence >= MASK_SEQUENCE)
                    {
                        switch (Options.SequenceOverflowStrategy)
                        {
                            case SequenceOverflowStrategy.SpinWait:
                                SpinWait.SpinUntil(() => _lastgen != GetTicks());
                                return CreateIdImpl(out exception); // Try again
                            case SequenceOverflowStrategy.Throw:
                            default:
                                exception = new SequenceOverflowException("Sequence overflow. Refusing to generate id for rest of tick");
                                return -1;
                        }
                    }
                    _sequence++;
                }
                else // We're in a new(er) "timeslot", so we can reset the sequence and store the new(er) "timeslot"
                {
                    _sequence = 0;
                    _lastgen = timestamp;
                }

                unchecked
                {
                    // If we made it here then no exceptions occurred; make sure we communicate that to the caller by setting `exception` to null
                    exception = null;
                    // Build id by shifting all bits into their place
                    return (timestamp << SHIFT_TIME)
                        + (_generatorid << SHIFT_GENERATOR)
                        + _sequence;
                }
            }
        }

        /// <summary>
        /// Returns information about an Id such as the sequence number, generator id and date/time the Id was generated
        /// based on the current <see cref="IdStructure"/> of the generator.
        /// </summary>
        /// <param name="id">The Id to extract information from.</param>
        /// <returns>Returns an <see cref="IdGen.Id" /> that contains information about the 'decoded' Id.</returns>
        /// <remarks>
        /// IMPORTANT: note that this method relies on the <see cref="IdStructure"/> and timesource; if the id was
        /// generated with a diffferent IdStructure and/or timesource than the current one the 'decoded' ID will NOT
        /// contain correct information.
        /// </remarks>
        public Id FromId(long id) =>
            // Deconstruct Id by unshifting the bits into the proper parts
            new Id(
                (int)(id & MASK_SEQUENCE),
                (int)((id >> SHIFT_GENERATOR) & MASK_GENERATOR),
                Options.TimeSource.Epoch.Add(TimeSpan.FromTicks(((id >> SHIFT_TIME) & MASK_TIME) * Options.TimeSource.TickDuration.Ticks))
            );

        /// <summary>
        /// Gets the number of ticks since the <see cref="ITimeSource"/>'s epoch.
        /// </summary>
        /// <returns>Returns the number of ticks since the <see cref="ITimeSource"/>'s epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetTicks() => Options.TimeSource.GetTicks();

        /// <summary>
        /// Returns a bitmask masking out the desired number of bits; a bitmask of 2 returns 000...000011, a bitmask of
        /// 5 returns 000...011111.
        /// </summary>
        /// <param name="bits">The number of bits to mask.</param>
        /// <returns>Returns the desired bitmask.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetMask(byte bits) => (1L << bits) - 1;

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
        public IEnumerator<long> GetEnumerator() => IdStream().GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates over Id's.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate over Id's.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}