using IdGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace IdGenTests;

[TestClass]
public class DatacenterMachineIdGeneratorIdCreatorTests

{
    [TestMethod]
    public void Create_ShouldCreateCorrectId()
    {
        Assert.AreEqual(0, DatacenterMachineIdGeneratorIdCreator.Create(0, 0, 4, 4));
        Assert.AreEqual(52394, DatacenterMachineIdGeneratorIdCreator.Create(204, 170, 8, 8));
        Assert.AreEqual(2147483647, DatacenterMachineIdGeneratorIdCreator.Create(536870911, 3, 29, 2));
        Assert.AreEqual(255, DatacenterMachineIdGeneratorIdCreator.Create(15, 15, 4, 4));
        Assert.AreEqual(255, DatacenterMachineIdGeneratorIdCreator.Create(127, 1, 7, 1));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Create_ShouldThrowOnMoreThan31TotalBits()
        => DatacenterMachineIdGeneratorIdCreator.Create(0, 0, 30, 2);

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Create_ShouldThrowOnDatacenterIdExceedingBits()
        => DatacenterMachineIdGeneratorIdCreator.Create(4, 0, 2, 8);

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Create_ShouldThrowOnWorkerIdExceedingBits()
        => DatacenterMachineIdGeneratorIdCreator.Create(0, 4, 8, 2);
}