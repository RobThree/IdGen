using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace IdGen.Configuration
{
    /// <summary>
    /// Helper class to get IdGen configuration from the application configuration.
    /// </summary>
    public static class AppConfigFactory
    {
        private static readonly ITimeSource defaulttimesource = new DefaultTimeSource(IdGeneratorOptions.DefaultEpoch);
        private static readonly ConcurrentDictionary<string, IdGenerator> _namedgenerators = new ConcurrentDictionary<string, IdGenerator>();

        /// <summary>
        /// Returns an instance of an <see cref="IdGenerator"/> based on the values in the corresponding idGenerator
        /// element in the idGenSection of the configuration file. The <see cref="DefaultTimeSource"/> is used to
        /// retrieve timestamp information.
        /// </summary>
        /// <param name="name">The name of the <see cref="IdGenerator"/> in the idGenSection.</param>
        /// <returns>
        /// An instance of an <see cref="IdGenerator"/> based on the values in the corresponding idGenerator
        /// element in the idGenSection of the configuration file.
        /// </returns>
        /// <remarks>
        /// When the <see cref="IdGenerator"/> doesn't exist it is created; any consequent calls to this method with
        /// the same name will return the same instance.
        /// </remarks>
        public static IdGenerator GetFromConfig(string name)
        {
            var result = _namedgenerators.GetOrAdd(name, (n) =>
            {
                var idgenerators = (ConfigurationManager.GetSection(IdGeneratorsSection.SectionName) as IdGeneratorsSection).IdGenerators;
                var idgen = idgenerators.OfType<IdGeneratorElement>().FirstOrDefault(e => e.Name.Equals(n, StringComparison.Ordinal));
                if (idgen != null)
                {
                    var ts = idgen.TickDuration == TimeSpan.Zero ? defaulttimesource : new DefaultTimeSource(idgen.Epoch, idgen.TickDuration);
                    var options = new IdGeneratorOptions(new IdStructure(idgen.TimestampBits, idgen.GeneratorIdBits, idgen.SequenceBits), ts, idgen.SequenceOverflowStrategy);
                    return new IdGenerator(idgen.Id, options);
                }

                throw new KeyNotFoundException();
            });

            return result;
        }
    }
}