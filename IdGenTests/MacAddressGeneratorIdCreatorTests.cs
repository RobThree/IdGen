using IdGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace IdGenTests;

[TestClass]
public class MacAddressGeneratorIdCreatorTests
{
    [TestMethod]
    public void Create_ShouldCreateCorrectId()
        // This doesn't really test anything, but it's here to make sure trying to gat a MacAddress doesn't throw
        => Assert.AreNotEqual(0, MacAddressGeneratorIdCreator.Create(31));

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Create_ShouldThrowOnMoreThan31Bits()
        => MacAddressGeneratorIdCreator.Create(32);


    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_ShouldThrowOnIncorrectMacAddress()
        => MacAddressGeneratorIdCreator.Create(new byte[3], 8);
}
