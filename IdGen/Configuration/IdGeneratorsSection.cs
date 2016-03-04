using System.Configuration;

namespace IdGen.Configuration
{
    /// <summary>
    /// Represents an IdGenerators section within a configuration file.
    /// </summary>
    public class IdGeneratorsSection : ConfigurationSection
    {
        /// <summary>
        /// The default name of the section.
        /// </summary>
        public const string SectionName = "idGenSection";

        /// <summary>
        /// The default name of the collection.
        /// </summary>
        private const string IdGensCollectionName = "idGenerators";

        /// <summary>
        /// Gets an <see cref="IdGeneratorsCollection"/> of all the <see cref="IdGeneratorElement"/> objects in all
        /// participating configuration files.
        /// </summary>
        [ConfigurationProperty(IdGensCollectionName)]
        [ConfigurationCollection(typeof(IdGeneratorsCollection), AddItemName = "idGenerator")]
        public IdGeneratorsCollection IdGenerators { get { return (IdGeneratorsCollection)base[IdGensCollectionName]; } }
    }
}