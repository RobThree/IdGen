using System;

namespace IdGen
{
    /// <summary>
    /// Holds information about a decoded id.
    /// </summary>
    public struct ID
    {
        /// <summary>
        /// Gets the sequence number of the id.
        /// </summary>
        public int SequenceNumber { get; private set; }
        
        /// <summary>
        /// Gets the generator id of the generator that generated the id.
        /// </summary>
        public int GeneratorId { get; private set; }
        
        /// <summary>
        /// Gets the date/time when the id was generated.
        /// </summary>
        public DateTimeOffset DateTimeOffset { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ID"/> struct.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the id.</param>
        /// <param name="generatorId">The generator id of the generator that generated the id.</param>
        /// <param name="dateTimeOffset">The date/time when the id was generated.</param>
        /// <returns></returns>
        internal static ID Create(int sequenceNumber, int generatorId, DateTimeOffset dateTimeOffset)
        {
            return new ID
            {
                SequenceNumber = sequenceNumber,
                GeneratorId = generatorId,
                DateTimeOffset = dateTimeOffset
            };
        }
    }
}
