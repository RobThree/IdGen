#if NETSTANDARD2_0_OR_GREATER

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace IdGen;

public enum IpAddressType
{
    IPv4 = 4,
    IPv6 = 6,
    IPvAny = 0
}

public static class IPAddressGeneratorIdCreator
{
    public static int Create(IpAddressType ipType, byte generatorIdBits)
        => Create(GetLocalIp(ipType), generatorIdBits);

    public static int Create(IpAddressType ipType, IdStructure idStructure)
        => Create(GetLocalIp(ipType), idStructure.GeneratorIdBits);

    public static int Create(IPAddress ip, IdStructure idStructure)
        => Create(ip, idStructure.GeneratorIdBits);

    public static int Create(IPAddress ip, byte generatorIdBits)
    {
        if (generatorIdBits is < 0 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(generatorIdBits), Translations.ERR_GENERATORID_CANNOT_EXCEED_31BITS);
        }

        var ipbytes = ip.GetAddressBytes().Reverse().ToArray();
        return BitConverter.ToInt32(ipbytes, 0) & ((1 << generatorIdBits) - 1) & 0x7FFFFFFF;
    }

    public static IPAddress GetLocalIp(IpAddressType ipAddressType)
        => Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(i => ipAddressType == IpAddressType.IPvAny || i.AddressFamily == (ipAddressType == IpAddressType.IPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6))
            ?? throw new InvalidOperationException("Unable to determine IP address");
}
#endif