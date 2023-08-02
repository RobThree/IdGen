using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace IdGen.Configuration;

/// <summary>
/// Represents a IdGenerators configuration element containing a collection of child elements.
/// </summary>
public class IdGeneratorsCollection : ConfigurationElementCollection, IReadOnlyCollection<ConfigurationElement>
{
    /// <summary>
    /// Creates a new <see cref="IdGeneratorElement"/>.
    /// </summary>
    /// <returns>A newly created <see cref="IdGeneratorElement"/>.</returns>
    protected override ConfigurationElement CreateNewElement() => new IdGeneratorElement();

    /// <summary>
    /// Gets the element key for a specified <see cref="IdGeneratorElement"/>.
    /// </summary>
    /// <param name="element">The <see cref="IdGeneratorElement"/> to return the key for.</param>
    /// <returns>An <see cref="object"/> that acts as the key for the specified <see cref="IdGeneratorElement"/>.</returns>
    protected override object GetElementKey(ConfigurationElement element) => ((IdGeneratorElement)element)?.Name;

    // Make compiler happy (CA1010)
    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public new IEnumerator<ConfigurationElement> GetEnumerator()
        => Enumerable.Range(0, Count).Select(BaseGet).GetEnumerator();
}