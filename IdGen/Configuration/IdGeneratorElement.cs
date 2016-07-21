using System;
using System.Configuration;
using System.Globalization;

namespace IdGen.Configuration
{
    /// <summary>
    /// Represents an IdGenerator configuration element. This class cannot be inherited.
    /// </summary>
    public sealed class IdGeneratorElement : ConfigurationElement
    {
        private readonly string[] DATETIMEFORMATS = { "yyyy-MM-dd\\THH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" };

        /// <summary>
        /// Gets/sets the name of the <see cref="IdGeneratorElement"/>.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Gets/sets the GeneratorId of the <see cref="IdGeneratorElement"/>.
        /// </summary>
        [ConfigurationProperty("id", IsRequired = true)]
        public int Id
        {
            get { return (int)this["id"]; }
            set { this["id"] = value; }
        }


        [ConfigurationProperty("epoch", IsRequired = true)]
        private string stringepoch
        {
            get { return (string)this["epoch"]; }
            set { this["epoch"] = value; }
        }

        /// <summary>
        /// Gets/sets the Epoch of the <see cref="IdGeneratorElement"/>.
        /// </summary>
        public DateTime Epoch
        {
            get { return DateTime.SpecifyKind(DateTime.ParseExact(this.stringepoch, DATETIMEFORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None), DateTimeKind.Utc); }
            set { this.stringepoch = value.ToString(DATETIMEFORMATS[0]); }
        }

        /// <summary>
        /// Gets/sets the <see cref="MaskConfig.TimestampBits"/> of the <see cref="IdGeneratorElement"/>.
        /// </summary>
        [ConfigurationProperty("timestampBits", IsRequired = true)]
        public byte TimestampBits
        {
            get { return (byte)this["timestampBits"]; }
            set { this["timestampBits"] = value; }
        }

        /// <summary>
        /// Gets/sets the <see cref="MaskConfig.GeneratorIdBits"/> of the <see cref="IdGeneratorElement"/>.
        /// </summary>
        [ConfigurationProperty("generatorIdBits", IsRequired = true)]
        public byte GeneratorIdBits
        {
            get { return (byte)this["generatorIdBits"]; }
            set { this["generatorIdBits"] = value; }
        }

        /// <summary>
        /// Gets/sets the <see cref="MaskConfig.SequenceBits"/> of the <see cref="IdGeneratorElement"/>.
        /// </summary>
        [ConfigurationProperty("sequenceBits", IsRequired = true)]
        public byte SequenceBits
        {
            get { return (byte)this["sequenceBits"]; }
            set { this["sequenceBits"] = value; }
        }

        /// <summary>
        /// Gets/sets the <see cref="ITimeSource.TickDuration"/> of the <see cref="IdGeneratorElement"/>.
        /// </summary>
        [ConfigurationProperty("tickDuration", IsRequired = false)]
        public TimeSpan TickDuration
        {
            get { return (TimeSpan)this["tickDuration"]; }
            set { this["tickDuration"] = value; }
        }
    }
}
