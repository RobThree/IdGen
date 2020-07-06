using System.Collections.Generic;

namespace IdGen
{
    /// <summary>
    /// Provides the interface for Id-generators.
    /// </summary>
    /// <typeparam name="T">The type for the generated ID's.</typeparam>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public interface IIdGenerator<T> : IEnumerable<T>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id.</returns>
        T CreateId();
    }
}
