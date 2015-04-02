using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IdGen
{
    public class IdGenerator
    {
        private int _sequence = 0;
        private long _lastgen = -1;

        private readonly DateTime _epoch;
        private readonly int _generatorId;

        private readonly long MASK_GENERATOR;
        private readonly long MASK_SEQUENCE;
        private readonly long MASK_TIME;
        private readonly int SHIFT_TIME;
        private readonly int SHIFT_GENERATOR;

        private object genlock = new object();

        public IdGenerator()
            : this(GetMachineHash()) { }

        public IdGenerator(int generatorId)
            : this(generatorId, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)) { }

        public IdGenerator(DateTime epoch)
            : this(GetMachineHash(), epoch) { }

        public IdGenerator(int generatorId, DateTime epoch)
            : this(generatorId, epoch, MaskConfig.Default) { }
        public IdGenerator(int generatorId, DateTime epoch, MaskConfig maskConfig)
        {
            if (maskConfig.TimestampBits + maskConfig.GeneratorIdBits + maskConfig.SequenceBits != 63)
                throw new InvalidOperationException("Number of bits used to generate ID's is not equal to 63");

            //TODO: Sanity-check mask-config for sane ranges...

            MASK_TIME = GetMask(maskConfig.TimestampBits);
            MASK_GENERATOR = GetMask(maskConfig.GeneratorIdBits);
            MASK_SEQUENCE = GetMask(maskConfig.SequenceBits);

            SHIFT_TIME = maskConfig.GeneratorIdBits + maskConfig.SequenceBits;
            SHIFT_GENERATOR = maskConfig.SequenceBits;

            _epoch = epoch;
            _generatorId = generatorId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long CreateId()
        {
            lock (genlock)
            {
                var timestamp = this.GetTime();

                //TODO: Benchmark below method and commented-out method
                //=================
                if (timestamp == _lastgen)
                {
                    _sequence++;
                    if (_sequence > MASK_SEQUENCE)
                    {
                        while (_lastgen == this.GetTime())
                            Thread.Sleep(0);
                        _sequence = 0;
                    }
                }
                else
                {
                    _sequence = 0;
                    _lastgen = timestamp;
                }

                //while (timestamp == _lastgen && _sequence >= MASK_SEQUENCE)
                //{
                //    Thread.Sleep(0);
                //    timestamp = GetTime();
                //}

                //_sequence = timestamp == _lastgen ? _sequence + 1 : 0;
                //_lastgen = timestamp;
                //=================
                
                unchecked
                {
                    return ((timestamp & MASK_TIME) << SHIFT_TIME)
                        + ((_generatorId & MASK_GENERATOR) << SHIFT_GENERATOR)
                        + (_sequence & MASK_SEQUENCE);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetTime()
        {
            return (long)(DateTime.UtcNow - _epoch).TotalMilliseconds;
        }

        private static int GetMachineHash()
        {
            return Environment.MachineName.GetHashCode();
        }

        private static long GetMask(byte bits)
        {
            return (1L << bits) - 1;
        }
    }
}
