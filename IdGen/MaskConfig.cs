using System;
namespace IdGen
{
    /// <summary>
    /// Specifies the number of bits to use for the different parts of an Id for an <see cref="IdGenerator"/>.
    /// </summary>
    public class MaskConfig
    {
        /// <summary>
        /// Gets number of bits to use for the timestamp part of the Id's to generate.
        /// </summary>
        public byte TimestampBits { get; private set; }

        /// <summary>
        /// Gets number of bits to use for the generator-id part of the Id's to generate.
        /// </summary>
        public byte GeneratorIdBits { get; private set; }

        /// <summary>
        /// Gets number of bits to use for the sequence part of the Id's to generate.
        /// </summary>
        public byte SequenceBits { get; private set; }

        /// <summary>
        /// Gets the total number of bits for the <see cref="MaskConfig"/>.
        /// </summary>
        public int TotalBits { get {  return this.TimestampBits + this.GeneratorIdBits + this.SequenceBits; } }

        /// <summary>
        /// Returns the maximum number of intervals for this mask configuration.
        /// </summary>
        public long MaxIntervals { get { return (1L << this.TimestampBits); } }

        /// <summary>
        /// Returns the maximum number of generators available for this mask configuration.
        /// </summary>
        public long MaxGenerators { get { return (1L << this.GeneratorIdBits); } }

        /// <summary>
        /// Returns the maximum number of sequential Id's for a time-interval (e.g. max. number of Id's generated 
        /// within a single interval).
        /// </summary>
        public long MaxSequenceIds { get { return (1L << this.SequenceBits); } }

        /// <summary>
        /// Gets a default <see cref="MaskConfig"/> with 41 bits for the timestamp part, 10 bits for the generator-id 
        /// part and 12 bits for the sequence part of the id.
        /// </summary>
        public static MaskConfig Default
        {
            get
            {
                return new MaskConfig(41, 10, 12);
            }
        }

        /// <summary>
        /// Initializes a bitmask configuration for <see cref="IdGenerator"/>s.
        /// </summary>
        /// <param name="timestampBits">Number of bits to use for the timestamp-part of Id's.</param>
        /// <param name="generatorIdBits">Number of bits to use for the generator-id of Id's.</param>
        /// <param name="sequenceBits">Number of bits to use for the sequence-part of Id's.</param>
        public MaskConfig(byte timestampBits, byte generatorIdBits, byte sequenceBits)
        {
            this.TimestampBits = timestampBits;
            this.GeneratorIdBits = generatorIdBits;
            this.SequenceBits = sequenceBits;
        }

        /// <summary>
        /// Calculates the last date for an Id before a 'wrap around' will occur in the timestamp-part of an Id for the
        /// given <see cref="MaskConfig"/>.
        /// </summary>
        /// <param name="epoch">The used epoch for the <see cref="IdGenerator"/> to use as offset.</param>
        /// <returns>The last date for an Id before a 'wrap around' will occur in the timestamp-part of an Id.</returns>
        public DateTime WraparoundDate(DateTime epoch)
        {
            return epoch.AddMilliseconds(this.MaxIntervals);
        }

        /// <summary>
        /// Calculates the interval at wich a 'wrap around' will occur in the timestamp-part of an Id for the given
        /// <see cref="MaskConfig"/>.
        /// </summary>
        /// <returns>
        /// The interval at wich a 'wrap around' will occur in the timestamp-part of an Id for the given
        /// <see cref="MaskConfig"/>.
        /// </returns>
        public TimeSpan WraparoundInterval()
        {
            return TimeSpan.FromMilliseconds(this.MaxIntervals);
        }
    }
}