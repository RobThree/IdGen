using IdGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace IdGenTests;

[TestClass]
public class IPAddressGeneratorIdCreatorTests
{
    [TestMethod]
    public void Create_ShouldCreateCorrectId()
    {
        //IPv4
        Assert.AreEqual(1084783405, IPAddressGeneratorIdCreator.Create(IPAddress.Parse("192.168.123.45"), 31));
        Assert.AreEqual(13, IPAddressGeneratorIdCreator.Create(IPAddress.Parse("192.168.123.45"), 4));
        Assert.AreEqual(1, IPAddressGeneratorIdCreator.Create(IPAddress.Parse("192.168.0.1"), 16));

        //IPv6
        Assert.AreEqual(1084783405, IPAddressGeneratorIdCreator.Create(IPAddress.Parse("::ffff:c0a8:7b2d"), 31));
        Assert.AreEqual(13, IPAddressGeneratorIdCreator.Create(IPAddress.Parse("::ffff:c0a8:7b2d"), 4));
        Assert.AreEqual(1, IPAddressGeneratorIdCreator.Create(IPAddress.Parse("::ffff:0:1"), 16));

        IPAddressGeneratorIdCreator.Create(IpAddressType.IPvAny, 16);
    }

    [TestMethod]
    public void Create_ShouldCreateCorrectIdForLocalIP() =>
        // This doesn't really test anything, but it's here to make sure GetLocalIp is invoked and doesn't throw
        Assert.AreNotEqual(0, IPAddressGeneratorIdCreator.Create(IpAddressType.IPvAny, 7));

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Create_ShouldThrowOnMoreThan31Bits()
        => IPAddressGeneratorIdCreator.Create(IPAddress.Parse("192.168.0.1"), 32);
}