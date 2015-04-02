
namespace IdGen
{
    public class MaskConfig
    {
        public byte TimestampBits { get; set; }
        public byte MachineIdBits { get; set; }
        public byte SequenceBits { get; set; }

        public static MaskConfig Default
        {
            get
            {
                return new MaskConfig { TimestampBits = 41, MachineIdBits = 10, SequenceBits = 12 };
            }
        }
    }
}
