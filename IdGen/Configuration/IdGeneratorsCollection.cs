using System.Configuration;

namespace IdGen.Configuration
{
    /// <summary>
    /// Represents a IdGenerators configuration element containing a collection of child elements.
    /// </summary>
    public class IdGeneratorsCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Creates a new <see cref="IdGeneratorElement"/>.
        /// </summary>
        /// <returns>A newly created <see cref="IdGeneratorElement"/>.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new IdGeneratorElement();
        }

        /// <summary>
        /// Gets the element key for a specified <see cref="IdGeneratorElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="IdGeneratorElement"/> to return the key for.</param>
        /// <returns>An <see cref="System.Object"/> that acts as the key for the specified <see cref="IdGeneratorElement"/>.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IdGeneratorElement)element).Name;
        }
    }
}
