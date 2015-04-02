using System;
using System.Threading;

namespace IdGen
{
    public class IdGenerator
    {
        private int _sequence = 0;
        private long _lastgen = -1;

        private readonly DateTime _epoch;
        private readonly int _machineId;

        private readonly long MASK_MACHINE;
        private readonly long MASK_SEQUENCE;
        private readonly long MASK_TIME;
        private readonly int SHIFT_TIME;
        private readonly int SHIFT_MACHINE;

        private object genlock = new object();

        public IdGenerator()
            : this(GetMachineHash()) { }

        public IdGenerator(int machineId)
            : this(machineId, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)) { }

        public IdGenerator(DateTime epoch)
            : this(GetMachineHash(), epoch) { }

        public IdGenerator(int machineId, DateTime epoch)
            : this(machineId, epoch, MaskConfig.Default) { }
        public IdGenerator(int machineId, DateTime epoch, MaskConfig maskConfig)
        {
            if (maskConfig.TimestampBits + maskConfig.MachineIdBits + maskConfig.SequenceBits != 63)
                throw new InvalidOperationException("Number of bits used to generate ID's is not equal to 63");

            //TODO: Sanity-check mask-config for sane ranges...

            MASK_TIME = GetMask(maskConfig.TimestampBits);
            MASK_MACHINE = GetMask(maskConfig.MachineIdBits);
            MASK_SEQUENCE = GetMask(maskConfig.SequenceBits);

            SHIFT_TIME = maskConfig.MachineIdBits + maskConfig.SequenceBits;
            SHIFT_MACHINE = maskConfig.SequenceBits;

            _epoch = epoch;
            _machineId = machineId;
        }

        public long CreateId()
        {
            return this.GenerateId();
        }

        private long GenerateId()
        {
            lock (genlock)
            {
                var timestamp = this.GetTime();

                //while (timestamp == _lastgen && _sequence >= MASK_SEQUENCE)
                //{
                //    Thread.Sleep(0);
                //    timestamp = GetTime();
                //}

                //_sequence = timestamp == _lastgen ? _sequence + 1 : 0;
                //_lastgen = timestamp;

                if (timestamp == _lastgen)
                {
                    _sequence++;
                    if (_sequence > MASK_SEQUENCE)
                    {
                        while (_lastgen == GetTime())
                            Thread.Sleep(0);
                        _sequence = 0;
                    }
                }
                else
                {
                    _sequence = 0;
                    _lastgen = timestamp;
                }

                unchecked
                {
                    return ((timestamp & MASK_TIME) << SHIFT_TIME)
                        + ((_machineId & MASK_MACHINE) << SHIFT_MACHINE)
                        + (_sequence & MASK_SEQUENCE);
                }
            }
        }

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
