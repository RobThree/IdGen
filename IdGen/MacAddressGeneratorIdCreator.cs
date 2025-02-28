#if NETSTANDARD2_0_OR_GREATER

using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace IdGen;

public static class MacAddressGeneratorIdCreator
{
    public static int Create(byte generatorIdBits)
    {
        var nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault()
            ?? throw new InvalidOperationException("Unable to determine MAC address");

        return Create(nic.GetPhysicalAddress().GetAddressBytes().Reverse().ToArray(), generatorIdBits);
    }

    public static int Create(byte[] macAddress, byte generatorIdBits)
        => generatorIdBits is < 0 or > 31
            ? throw new ArgumentOutOfRangeException(nameof(generatorIdBits), Translations.ERR_GENERATORID_CANNOT_EXCEED_31BITS)
            : macAddress.Length != 6
            ? throw new ArgumentException("MAC address must be 6 bytes long", nameof(macAddress))
            : BitConverter.ToInt32(macAddress, 0) & ((1 << generatorIdBits) - 1) & 0x7FFFFFFF;
}
#endif