using System;

namespace IdGen;

public static class DatacenterMachineIdGeneratorIdCreator
{
    public static int Create(int datacenterid, int workerid, byte datacenterbits, byte workerbits)
        => datacenterbits + workerbits > 31
            ? throw new InvalidOperationException("The total number of datacenter and worker bits should not exceed 31")
            : datacenterid < 0 || datacenterid >= (1 << datacenterbits)
            ? throw new ArgumentOutOfRangeException(nameof(datacenterid), $"Datacenter Id should be between 0 and {(1 << datacenterbits) - 1}")
            : workerid < 0 || workerid >= (1 << workerbits)
            ? throw new ArgumentOutOfRangeException(nameof(workerid), $"Worker Id should be between 0 and {(1 << workerbits) - 1}")
            : (datacenterid << workerbits) | workerid;
}