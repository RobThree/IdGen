using System;

namespace IdGen
{
    public struct ID
    {
        public int Sequence { get; private set; }
        public int Generator { get; private set; }
        public DateTimeOffset DateTimeOffset { get; private set; }

        public static ID Create(int sequence, int generator, DateTimeOffset dateTimeOffset)
        {
            return new ID
            {
                Sequence = sequence,
                Generator = generator,
                DateTimeOffset = dateTimeOffset
            };
        }
    }
}
