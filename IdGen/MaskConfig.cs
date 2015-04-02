
namespace IdGen
{
    public class MaskConfig
    {
        public byte TimestampBits { get; set; }
        public byte GeneratorIdBits { get; set; }
        public byte SequenceBits { get; set; }

        public static MaskConfig Default
        {
            get
            {
                return new MaskConfig { TimestampBits = 41, GeneratorIdBits = 10, SequenceBits = 12 };
            }
        }
    }
}
