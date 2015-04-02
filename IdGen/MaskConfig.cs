namespace IdGen
{
    /// <summary>
    /// Specifies the number of bits to use for the different parts of an Id for an <see cref="IdGenerator"/>.
    /// </summary>
    public class MaskConfig
    {
        /// <summary>
        /// Gets/sets number of bits to use for the timestamp part of the Id's to generate.
        /// </summary>
        public byte TimestampBits { get; set; }
        
        /// <summary>
        /// Gets/sets number of bits to use for the generator-id part of the Id's to generate.
        /// </summary>
        public byte GeneratorIdBits { get; set; }

        /// <summary>
        /// Gets/sets number of bits to use for the sequence part of the Id's to generate.
        /// </summary>
        public byte SequenceBits { get; set; }

        /// <summary>
        /// Gets a default <see cref="MaskConfig"/> with 41 bits for the timestamp part, 10 bits for the generator-id 
        /// part and 12 bits for the sequence part of the id.
        /// </summary>
        public static MaskConfig Default
        {
            get
            {
                return new MaskConfig { TimestampBits = 41, GeneratorIdBits = 10, SequenceBits = 12 };
            }
        }
    }
}
