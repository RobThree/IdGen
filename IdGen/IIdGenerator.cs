using System;
using System.Collections.Generic;

namespace IdGen
{
    /// <summary>
    /// Provides the interface for Id-generators.
    /// </summary>
    /// <typeparam name="T">The type for the generated ID's.</typeparam>
    public interface IIdGenerator<T> : IEnumerable<T>
    {
        /// <summary>
        /// Creates a new Id.
        /// </summary>
        /// <returns>Returns an Id.</returns>
        T CreateId();

        /// <summary>
        /// Gets the <see cref="ITimeSource"/> for the <see cref="IIdGenerator{T}"/>.
        /// </summary>
        ITimeSource TimeSource { get; }

        /// <summary>
        /// Gets the epoch for the <see cref="IIdGenerator{T}"/>.
        /// </summary>
        DateTimeOffset Epoch { get; }

        /// <summary>
        /// Gets the <see cref="MaskConfig"/> for the <see cref="IIdGenerator{T}"/>.
        /// </summary>
        MaskConfig MaskConfig { get; }
    }
}
