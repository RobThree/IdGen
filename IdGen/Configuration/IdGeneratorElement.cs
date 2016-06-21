using System;
#if NET45
using System.Configuration;
#endif
using System.Globalization;

namespace IdGen.Configuration
{
    /// <summary>
    /// Represents an IdGenerator configuration element. This class cannot be inherited.
    /// </summary>
    public sealed class IdGeneratorElement
#if NET45
        : ConfigurationElement
#endif
    {
#if NET45
        private readonly string[] DATETIMEFORMATS = { "yyyy-MM-dd\\THH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" };
#endif

        /// <summary>
        /// Gets/sets the name of the <see cref="IdGeneratorElement"/>.
        /// </summary>
#if NET45
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
#else
        public string Name { get; set; }
#endif
        /// <summary>
        /// Gets/sets the GeneratorId of the <see cref="IdGeneratorElement"/>.
        /// </summary>
#if NET45
        [ConfigurationProperty("id", IsRequired = true)]
        public int Id
        {
            get { return (int)this["id"]; }
            set { this["id"] = value; }
        }
#else
        public int Id { get; set; }
#endif

#if NET45
        [ConfigurationProperty("epoch", IsRequired = true)]
        private string stringepoch
        {
            get { return (string)this["epoch"]; }
            set { this["epoch"] = value; }
        }
#endif

        /// <summary>
        /// Gets/sets the Epoch of the <see cref="IdGeneratorElement"/>.
        /// </summary>
#if NET45
        public DateTime Epoch
        {
            get { return DateTime.SpecifyKind(DateTime.ParseExact(this.stringepoch, DATETIMEFORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None), DateTimeKind.Utc); }
            set { this.stringepoch = value.ToString(DATETIMEFORMATS[0]); }
        }
#else
        public DateTime Epoch { get; set; }
#endif

        /// <summary>
        /// Gets/sets the <see cref="MaskConfig.TimestampBits"/> of the <see cref="IdGeneratorElement"/>.
        /// </summary>
#if NET45
        [ConfigurationProperty("timestampBits", IsRequired = true)]
        public byte TimestampBits
        {
            get { return (byte)this["timestampBits"]; }
            set { this["timestampBits"] = value; }
        }
#else
        public byte TimestampBits { get; set; }
#endif

        /// <summary>
        /// Gets/sets the <see cref="MaskConfig.GeneratorIdBits"/> of the <see cref="IdGeneratorElement"/>.
        /// </summary>
#if NET45
        [ConfigurationProperty("generatorIdBits", IsRequired = true)]
        public byte GeneratorIdBits
        {
            get { return (byte)this["generatorIdBits"]; }
            set { this["generatorIdBits"] = value; }
        }
#else
        public byte GeneratorIdBits { get; set; }
#endif

        /// <summary>
        /// Gets/sets the <see cref="MaskConfig.SequenceBits"/> of the <see cref="IdGeneratorElement"/>.
        /// </summary>
#if NET45
        [ConfigurationProperty("sequenceBits", IsRequired = true)]
        public byte SequenceBits
        {
            get { return (byte)this["sequenceBits"]; }
            set { this["sequenceBits"] = value; }
        }
#else
        public byte SequenceBits { get; set; }
#endif
    }
}
