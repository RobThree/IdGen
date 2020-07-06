using System;

namespace IdGen
{
    /// <summary>
    /// Holds information about a decoded id.
    /// </summary>
    public struct Id : IEquatable<Id>
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
        /// Initializes a new instance of the <see cref="Id"/> struct.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the id.</param>
        /// <param name="generatorId">The generator id of the generator that generated the id.</param>
        /// <param name="dateTimeOffset">The date/time when the id was generated.</param>
        /// <returns></returns>
        internal Id(int sequenceNumber, int generatorId, DateTimeOffset dateTimeOffset)
        {
            SequenceNumber = sequenceNumber;
            GeneratorId = generatorId;
            DateTimeOffset = dateTimeOffset;
        }

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>true if <paramref name="obj"/> is a <see cref="Id"/> that has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Id)
                return Equals((Id)obj);
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => Tuple.Create(DateTimeOffset, GeneratorId, SequenceNumber).GetHashCode();

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Id"/> objects are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if left and right are equal; otherwise, false.</returns>
        public static bool operator ==(Id left, Id right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Id"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if left and right are not equal; otherwise, false.</returns>
        public static bool operator !=(Id left, Id right) => !(left == right);

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="Id"/> object represent the same value.
        /// </summary>
        /// <param name="other">An <see cref="Id"/> to compare to this instance.</param>
        /// <returns>true if <paramref name="other"/> is equal to this instance; otherwise, false.</returns>
        public bool Equals(Id other) => GeneratorId == other.GeneratorId
                 && DateTimeOffset == other.DateTimeOffset
                 && SequenceNumber == other.SequenceNumber;
    }
}