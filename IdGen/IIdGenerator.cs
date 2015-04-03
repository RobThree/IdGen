using System.Collections.Generic;

namespace IdGen
{
    /// <summary>
    /// Provides the interface for Id-generators.
    /// </summary>
    interface IIdGenerator<T> : IEnumerable<T>
    {
        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id.</returns>
        T CreateId();
    }
}
